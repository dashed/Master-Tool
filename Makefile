# ─── Environment ──────────────────────────────────────────────────────
DOTNET_ROOT  ?= $(HOME)/.dotnet
DOTNET       := $(DOTNET_ROOT)/dotnet
export DOTNET_ROOT
export PATH  := $(DOTNET_ROOT):$(DOTNET_ROOT)/tools:$(PATH)

# ─── Projects ─────────────────────────────────────────────────────────
SOLUTION     := MasterTool.sln
SRC_PROJECT  := src/MasterTool/MasterTool.csproj
TEST_PROJECT := tests/MasterTool.Tests/MasterTool.Tests.csproj
CONFIGURATION ?= Release

# ─── Directories ──────────────────────────────────────────────────────
SRC_DIR      := src
TEST_DIR     := tests
LIBS_DIR     := libs
BUILD_DIR    := build

# ─── SPT Installation ────────────────────────────────────────────────
SPT_DIR      ?= /mnt/e/sp_tarkov/40
PLUGIN_DIR   := $(SPT_DIR)/BepInEx/plugins

.DEFAULT_GOAL := help
.PHONY: help all ci version-check restore build build-tests test deploy install clean format format-check lint lint-fix

# ─── Meta ─────────────────────────────────────────────────────────────

help: ## Show available targets
	@echo "Usage: make <target>"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-14s\033[0m %s\n", $$1, $$2}'

all: format-check lint test ## Run format-check, lint, and test

ci: version-check restore format-check lint build-tests test ## Full CI pipeline

version-check: ## Verify CHANGELOG and plugin versions match
	@CHANGELOG_VER=$$(grep -m1 -oP '## \[\K[0-9]+\.[0-9]+\.[0-9]+' CHANGELOG.md); \
	PLUGIN_VER=$$(grep -oP 'BepInPlugin\("com\.master\.tools",\s*"[^"]*",\s*"\K[0-9]+\.[0-9]+\.[0-9]+' src/MasterTool/Plugin/MasterToolPlugin.cs); \
	echo "CHANGELOG version: $$CHANGELOG_VER"; \
	echo "Plugin version:    $$PLUGIN_VER"; \
	if [ "$$CHANGELOG_VER" != "$$PLUGIN_VER" ]; then \
		echo "Error: Version mismatch! CHANGELOG=$$CHANGELOG_VER Plugin=$$PLUGIN_VER"; \
		exit 1; \
	fi

# ─── Build ────────────────────────────────────────────────────────────

restore: ## Restore NuGet packages
	$(DOTNET) restore $(SOLUTION)

build: ## Build the plugin DLL (requires libs/)
	@if [ ! -d "$(LIBS_DIR)" ]; then \
		echo "Error: $(LIBS_DIR)/ directory not found."; \
		echo "Copy the required game DLLs from your SPT installation."; \
		echo "See src/MasterTool/MasterTool.csproj for the full list."; \
		exit 1; \
	fi
	$(DOTNET) build $(SRC_PROJECT) -c $(CONFIGURATION) --nologo

build-tests: ## Build the test project
	$(DOTNET) build $(TEST_PROJECT) -c $(CONFIGURATION) --nologo

# ─── Deploy ───────────────────────────────────────────────────────────

deploy: build ## Build and show deploy-ready files in build/
	@echo "Deploy-ready files in $(BUILD_DIR)/:"
	@ls -lh $(BUILD_DIR)/MasterTool.dll $(BUILD_DIR)/MasterTool.Core.dll
	@echo ""
	@echo "Copy both DLLs to your SPT plugins folder:"
	@echo "  cp $(BUILD_DIR)/MasterTool.dll $(BUILD_DIR)/MasterTool.Core.dll <SPT>/BepInEx/plugins/"
	@echo "Or run: make install"

install: deploy ## Deploy and install to SPT_DIR
	@if [ ! -d "$(PLUGIN_DIR)" ]; then \
		echo "Error: Plugin directory not found: $(PLUGIN_DIR)"; \
		echo "Set SPT_DIR to your SPT installation, e.g.:"; \
		echo "  make install SPT_DIR=/mnt/e/sp_tarkov/40"; \
		exit 1; \
	fi
	@echo "Installing to $(PLUGIN_DIR)..."
	@cp $(BUILD_DIR)/MasterTool.dll $(BUILD_DIR)/MasterTool.Core.dll "$(PLUGIN_DIR)/"
	@echo "Installed MasterTool.dll + MasterTool.Core.dll to $(PLUGIN_DIR)/"

# ─── Test ─────────────────────────────────────────────────────────────

test: ## Run unit tests
	$(DOTNET) test $(TEST_PROJECT) -c $(CONFIGURATION) --nologo

# ─── Format ───────────────────────────────────────────────────────────

format: ## Auto-format code with CSharpier
	csharpier format $(SRC_DIR) $(TEST_DIR)

format-check: ## Check code formatting (CI-safe, no changes)
	csharpier check $(SRC_DIR) $(TEST_DIR)

# ─── Lint ─────────────────────────────────────────────────────────────

lint: ## Check code style against .editorconfig
	$(DOTNET) format $(TEST_PROJECT) --verify-no-changes --no-restore -v diag

lint-fix: ## Auto-fix code style issues
	$(DOTNET) format $(TEST_PROJECT) --no-restore -v diag

# ─── Clean ────────────────────────────────────────────────────────────

clean: ## Remove build artifacts
	$(DOTNET) clean $(SOLUTION) --nologo -v q 2>/dev/null || true
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true
