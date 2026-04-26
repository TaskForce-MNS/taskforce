# ==========================================
# ⚙️ VARIABLES (À modifier selon tes besoins)
# ==========================================
REGISTRY ?= beselimius
VERSION ?= v1.0.0

# Déclaration de toutes les commandes disponibles
.PHONY: start stop build clean build-prod push-prod deploy

# ==========================================
# 🏃‍♂️ LE QUOTIDIEN (À utiliser tous les matins)
# ==========================================

# Démarrage ultra-rapide (Utilise les images existantes)
start:
	@echo "🚀 Démarrage rapide de l'environnement..."
	docker compose -f docker-compose.dev.yml up -d

# Arrêt simple (Fige l'état pour le lendemain)
stop:
	@echo "🛑 Arrêt des services..."
	docker compose -f docker-compose.dev.yml stop

# ==========================================
# 🛠️ LA MAINTENANCE (En cas de modif architecture/packages)
# ==========================================

# Reconstruction complète
build:
	@echo "🏗️ Reconstruction des images de développement..."
	docker compose -f docker-compose.dev.yml up -d --build

# Nettoie tout depuis zéro (y compris les volumes capricieux)
clean:
	@echo "🧹 Nettoyage complet (Conteneurs + Volumes)..."
	docker compose -f docker-compose.dev.yml down -v

# ==========================================
# 🚀 LA PRODUCTION (Pour le déploiement)
# ==========================================

# 1. Construit toutes les images de prod
build-prod:
	@echo "🏗️ Construction des images de production ($(VERSION))..."
	docker build -f apps/Api.Back/Dockerfile -t $(REGISTRY)/taskforce_api:$(VERSION) ./apps/Api.Back
	docker build -f apps/web-app/Dockerfile -t $(REGISTRY)/taskforce_webapp:$(VERSION) ./apps/web-app
	docker build -f apps/landing-page/Dockerfile -t $(REGISTRY)/taskforce_landing:$(VERSION) ./apps/landing-page

# 2. Pousse tout vers le registre
push-prod:
	@echo "☁️ Envoi vers le registre $(REGISTRY)..."
	docker push $(REGISTRY)/taskforce_api:$(VERSION)
	docker push $(REGISTRY)/taskforce_webapp:$(VERSION)
	docker push $(REGISTRY)/taskforce_landing:$(VERSION)

# 3. La commande "Bouton Rouge" qui fait tout d'un coup
deploy: build-prod push-prod
	@echo "✅ Déploiement de la version $(VERSION) terminé avec succès !"