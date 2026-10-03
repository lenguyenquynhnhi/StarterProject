.DEFAULT_GOAL := help
SHELL := /bin/bash

API := src/DevOpsLab.WebApi
INFRA := src/DevOpsLab.Infrastructure

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-16s\033[0m %s\n", $$1, $$2}'

setup: ## One-command environment setup (database + migrations + build + tests)
	@bash scripts/setup.sh

db-up: ## Start the local SQL Server container
	docker compose up -d

db-down: ## Stop the local SQL Server container (keeps data)
	docker compose down

db-reset: ## Stop the database AND delete its volume
	docker compose down -v

migrate: ## Apply Entity Framework migrations
	dotnet ef database update --project $(INFRA) --startup-project $(API)

run: ## Run the Web API on http://localhost:5080
	dotnet run --project $(API)

build: ## Restore and build the whole solution
	dotnet build DevOpsLab.sln -c Debug

test: ## Run the full test suite
	dotnet test DevOpsLab.sln -c Debug

coverage: ## Run tests and collect code coverage
	dotnet test DevOpsLab.sln -c Debug --collect:"XPlat Code Coverage" --results-directory ./TestResults

health: ## Probe both health endpoints of a locally running API
	@echo "--- /healthz/live ---"  && curl -fsS http://localhost:5080/healthz/live  && echo
	@echo "--- /healthz/ready ---" && curl -fsS http://localhost:5080/healthz/ready && echo

.PHONY: help setup db-up db-down db-reset migrate run build test coverage health
