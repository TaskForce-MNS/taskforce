# ==========================================
# ⚙️ VARIABLES & CONFIGURATION
# ==========================================

ifneq (,$(wildcard ./.env))
    include .env
    export
endif

export DOCKER_UID := $(shell id -u)
export DOCKER_GID := $(shell id -g)

REGISTRY ?= beselimius
VERSION  ?= v1.0.0

.PHONY: start stop build clean \
	pnpm pnpm-landing install-web install-landing \
	logs shell-api shell-db shell-redis \
	db-check db-add-migration db-migrate db-rollback db-backup \
	test test-build coverage-report lint scan-secrets scan-files scan-secrets-debug \
	build-prod push-prod ci-deploy \
	staging-pull staging-up staging-down

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
# 📦 PNPM
# ==========================================

pnpm:
	@test -n "$(CMD)" || (echo "❌ Usage: make pnpm CMD=\"add recharts\"" && exit 1)
	docker exec -it taskforce_webapp pnpm $(CMD)

pnpm-landing:
	@test -n "$(CMD)" || (echo "❌ Usage: make pnpm-landing CMD=\"add astro-icon\"" && exit 1)
	docker exec -it taskforce_landing pnpm $(CMD)

install-web:
	cd apps/web-app && pnpm install

install-landing:
	cd apps/landing-page && pnpm install
# ==========================================
# 🔍 DEBUG & INSPECTION 
# ==========================================
logs:
	docker compose -f docker-compose.dev.yml logs -f $(filter-out $@,$(MAKECMDGOALS))

shell-web:
	docker exec -it taskforce_webapp sh

shell-landing:
	docker exec -it taskforce_landing sh

shell-api:
	docker exec -it taskforce_api bash

shell-db:
	@docker exec -it taskforce_db psql -U $(DB_USER) -d $(DB_NAME)

shell-redis:
	@echo "🔌 Connexion sécurisée à Redis..."
	@docker exec -it -e REDISCLI_AUTH=$(REDIS_PASSWORD) taskforce_redis redis-cli

# ==========================================
# 🗃️ BASE DE DONNÉES 
# ==========================================
db-check:
	@echo "🔍 Vérification des changements de modèle..."
	@docker exec taskforce_api dotnet ef migrations has-pending-model-changes \
		&& echo "✅ Aucun changement en attente." \
		|| echo "⚠️  Des changements ont été détectés. Lancez : make db-add-migration NAME=NomDeLaMigration"

db-add-migration:
	@echo "DEBUG: NAME reçu = [$(CMD)]"
	@test -n "$(CMD)" || (echo "❌ Usage: make db-add-migration NAME=NomDeLaMigration" && exit 1)
	@echo "📝 Création de la migration $(CMD)..."
	docker exec -it taskforce_api dotnet ef migrations add $(CMD)
	
db-migrate:
	@echo "🗃️ Application des migrations EF Core..."
	docker exec -it taskforce_api dotnet ef database update

db-rollback:
	@test -n "$(MIGRATION)" || (echo "❌ Usage: make db-rollback MIGRATION=NomDeLaMigration" && exit 1)
	@echo "⏪ Rollback vers $(MIGRATION)..."
	@docker exec -it taskforce_api dotnet ef database update $(MIGRATION)

db-backup:
	@echo "💾 Backup de la base de données..."
	mkdir -p backups
	@docker exec taskforce_db pg_dump -U $(DB_USER) $(DB_NAME) > backups/backup_$(shell date +%Y%m%d_%H%M%S).sql

# ==========================================
# 🧪 QUALITÉ 
# ==========================================
test: test-build run-tests coverage-report

test-build:
	@echo "🛠️ Build de l'image de test..."
	docker compose -f docker-compose.test.yml build api-unit-tests

run-tests:
	@echo "📁 Préparation du dossier de résultats..."
	mkdir -p TestResults
	chmod 777 TestResults
	@echo "🧪 Lancement des tests et de la couverture..."
	docker compose -f docker-compose.test.yml run --rm api-unit-tests

coverage-report:
	@echo "📊 Rapport de couverture disponible sur http://localhost:8000"
	cd TestResults/HtmlReport && python3 -m http.server 8000
	
lint:
	@echo "🔍 Vérification du code frontend..."
	docker exec taskforce_webapp pnpm lint

# 🔍 Secret scan avec TruffleHog
scan-secrets:
	trufflehog git file://$(PWD) --no-update

# 🔍 Scan rapide filesystem (sans Git history)
scan-files:
	TRUFFLEHOG_NO_UPDATE=true trufflehog filesystem .

# 🧼 Version verbose (debug utile)
scan-secrets-debug:
	trufflehog git file://$(PWD) --no-update --debug
# ==========================================
# 🚀 PRODUCTION
# ==========================================
build-prod:
	@echo "🏗️ Build production ($(VERSION))..."
	docker build -f apps/Api.Back/Dockerfile.prod      -t $(REGISTRY)/taskforce_api:$(VERSION)     ./apps/Api.Back
	docker build -f apps/web-app/Dockerfile.prod       -t $(REGISTRY)/taskforce_webapp:$(VERSION)  .
	docker build -f apps/landing-page/Dockerfile.prod  -t $(REGISTRY)/taskforce_landing:$(VERSION) .
	# Tag latest
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

ci-deploy: build-prod push-prod
	@echo "✅ Images $(VERSION) construites et poussées sur le registre !"

# ==========================================
# 🚀 STAGING (Commandes à exécuter SUR LA VM)
# ==========================================
staging-pull:
	@echo "📥 Téléchargement des dernières images depuis le registre..."
	REGISTRY=$(REGISTRY) VERSION=$(VERSION) docker compose -f docker-compose.staging.yml --env-file .env.staging pull

staging-up: staging-pull
	@echo "🛑 Nettoyage de l'ancien environnement..."
	REGISTRY=$(REGISTRY) VERSION=$(VERSION) docker compose -f docker-compose.staging.yml --env-file .env.staging down
	@echo "🚀 Démarrage de l'environnement de Staging..."
	REGISTRY=$(REGISTRY) VERSION=$(VERSION) docker compose -f docker-compose.staging.yml --env-file .env.staging up -d
	@echo "✅ Staging en ligne et à jour !"

staging-down:
	@echo "🛑 Arrêt du Staging..."
	REGISTRY=$(REGISTRY) VERSION=$(VERSION) docker compose -f docker-compose.staging.yml --env-file .env.staging down
%:
	@: