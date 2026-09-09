#!/bin/bash
set -e

echo "🎓 1. Installation de ReportGenerator..."
export PATH="$PATH:/workspace/.dotnet/tools"

dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.5.11 || true

echo "📁 2. Préparation des résultats..."
mkdir -p /workspace/TestResults/Raw
mkdir -p /workspace/TestResults/HtmlReport

echo "🧪 3. Lancement des tests avec couverture..."

dotnet test Api.Back.UnitTests.csproj -- \
  --coverage \
   --coverage-settings /workspace/Api.Back.UnitTests/coverage.runsettings \
  --coverage-output-format cobertura \
  --coverage-output /workspace/TestResults/Raw/coverage.cobertura.xml

echo "📊 4. Génération du rapport HTML..."

reportgenerator \
  -reports:"/workspace/TestResults/Raw/coverage.cobertura.xml" \
  -targetdir:"/workspace/TestResults/HtmlReport" \
  -reporttypes:Html

echo "✅ Rapport généré avec succès !"
echo "📁 HTML : /workspace/TestResults/HtmlReport/index.html"