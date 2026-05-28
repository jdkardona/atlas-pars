# Configuración del proveedor
provider "azurerm" {
  features {}
}

# 1. Resource Group
resource "azurerm_resource_group" "atlas_rg" {
  name     = "rg-atlas-pars-prod"
  location = "East US"
}

# 2. Azure Container App Environment
resource "azurerm_container_app_environment" "atlas_env" {
  name                = "cae-atlas-pars"
  resource_group_name = azurerm_resource_group.atlas_rg.name
  location            = azurerm_resource_group.atlas_rg.location
}

# 3. PostgreSQL Flexible Server
resource "azurerm_postgresql_flexible_server" "atlas_db" {
  name                   = "psql-atlas-pars"
  resource_group_name    = azurerm_resource_group.atlas_rg.name
  location               = azurerm_resource_group.atlas_rg.location
  sku_name               = "B_Standard_B1ms"
  version                = "16"
  administrator_login    = "atlasadmin"
  administrator_password = "Password123!" # Nota: En producción, usaríamos KeyVault
}