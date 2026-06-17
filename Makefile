# ==========================================
# ⚙️ VARIABLES & CONFIGURATION
# ==========================================

ifneq (,$(wildcard ./.env))
    include .env
    export
endif

REGISTRY ?= beselimius
VERSION  ?= v1.0.0

.PHONY: start stop build clean logs shell-api shell-db shell-redis db-migrate db-rollback db-backup test lint build-prod push-prod deploy

# ==========================================
# 🏃‍♂️ QUOTIDIEN
# ==========================================
start:
	@echo "🚀 Démarrage rapide..."
	docker compose -f docker-compose.dev.yml up -d

stop:
	@echo "🛑 Arrêt des services..."
	docker compose -f docker-compose.dev.yml stop

build:
	@echo "🏗️ Reconstruction..."
	docker compose -f docker-compose.dev.yml up -d --build

clean:
	@echo "🧹 Nettoyage complet..."
	docker compose -f docker-compose.dev.yml down -v

# ==========================================
# 🔍 DEBUG & INSPECTION 
# ==========================================
logs:
	docker compose -f docker-compose.dev.yml logs -f $(filter-out $@,$(MAKECMDGOALS))

shell-api:
	docker exec -it taskforce_api bash

shell-db:
	docker exec -it taskforce_db psql -U $(DB_USER) -d $(DB_NAME)

shell-redis:
	docker exec -it taskforce_redis redis-cli -a $(REDIS_PASSWORD)

# ==========================================
# 🗃️ BASE DE DONNÉES 
# ==========================================
db-add-migration:
	@test -n "$(NAME)" || (echo "❌ Usage: make db-add-migration NAME=NomDeLaMigration" && exit 1)
	@echo "📝 Création de la migration $(NAME)..."
	docker exec -it taskforce_api dotnet ef migrations add $(NAME)
	
db-migrate:
	@echo "🗃️ Application des migrations EF Core..."
	docker exec -it taskforce_api dotnet ef database update

db-rollback:
	@echo "⏪ Rollback de la dernière migration..."
	docker exec -it taskforce_api dotnet ef database update PreviousMigrationName

db-backup:
	@echo "💾 Backup de la base de données..."
	mkdir -p backups
	docker exec taskforce_db pg_dump -U $(DB_USER) $(DB_NAME) > backups/backup_$(shell date +%Y%m%d_%H%M%S).sql

# ==========================================
# 🧪 QUALITÉ 
# ==========================================
test:
	@echo "🧪 Lancement des tests..."
	docker exec taskforce_api dotnet test apps/Api.Back.UnitTests/Api.Back.UnitTests.csproj

lint:
	@echo "🔍 Vérification du code frontend..."
	docker exec taskforce_webapp pnpm lint

# ==========================================
# 🚀 PRODUCTION
# ==========================================
build-prod:
	@echo "🏗️ Build production ($(VERSION))..."
	docker build -f apps/Api.Back/Dockerfile      -t $(REGISTRY)/taskforce_api:$(VERSION)     ./apps/Api.Back
	docker build -f apps/web-app/Dockerfile        -t $(REGISTRY)/taskforce_webapp:$(VERSION)  ./apps/web-app
	docker build -f apps/landing-page/Dockerfile   -t $(REGISTRY)/taskforce_landing:$(VERSION) ./apps/landing-page
	# Tag latest aussi
	docker tag $(REGISTRY)/taskforce_api:$(VERSION)     $(REGISTRY)/taskforce_api:latest
	docker tag $(REGISTRY)/taskforce_webapp:$(VERSION)  $(REGISTRY)/taskforce_webapp:latest
	docker tag $(REGISTRY)/taskforce_landing:$(VERSION) $(REGISTRY)/taskforce_landing:latest

push-prod:
	@echo "☁️ Push vers $(REGISTRY)..."
	docker push $(REGISTRY)/taskforce_api:$(VERSION)
	docker push $(REGISTRY)/taskforce_webapp:$(VERSION)
	docker push $(REGISTRY)/taskforce_landing:$(VERSION)
	docker push $(REGISTRY)/taskforce_api:latest
	docker push $(REGISTRY)/taskforce_webapp:latest
	docker push $(REGISTRY)/taskforce_landing:latest

deploy: build-prod push-prod
	@echo "✅ Version $(VERSION) déployée !"

%:
	@: