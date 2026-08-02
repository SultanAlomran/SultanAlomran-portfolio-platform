# Contributing

This document defines the development workflow and coding standards for the Sultan Alomran Portfolio Platform. The repository is a private engineering portfolio developed by Sultan Alomran with support from ChatGPT and Codex; this guide serves as the project's development playbook rather than an open-source contribution guide.

---

## Development Workflow

Development follows this branch hierarchy:

```text
main
└── dev
    └── feature/*
```

Never commit directly to `main`. Develop each feature in its own feature branch, created from `dev`. Whenever practical, a feature branch should deliver a complete vertical slice.

After a feature is complete, follow this promotion path:

```text
Feature Branch
→ Pull Request
→ Merge into dev
→ Testing
→ Merge dev into main
```

## Feature Branch Naming

Use the `feature/<name>` pattern. Approved feature branch names are:

- `feature/solution-foundation`
- `feature/persistence-foundation`
- `feature/public-foundation`
- `feature/homepage`
- `feature/projects`
- `feature/visual-handbook`
- `feature/about`
- `feature/experience`
- `feature/contact`
- `feature/admin-foundation`
- `feature/admin-content`
- `feature/analytics`
- `feature/deployment`

## Commit Convention

Use [Conventional Commits](https://www.conventionalcommits.org/) with concise, imperative descriptions.

- `feat: add project filtering`
- `fix: correct contact form validation`
- `docs: update database specification`
- `refactor: simplify project query handler`
- `test: add homepage integration tests`
- `style: apply consistent code formatting`
- `chore: update development dependencies`

## Pull Request Checklist

Every pull request should include:

- [ ] Purpose
- [ ] Summary of changes
- [ ] Testing performed
- [ ] Documentation updated, if required
- [ ] No unrelated changes

## Architecture Principles

Follow the approved `docs/Project_00_Master_Document.md`, `docs/Database_Specification.md`, and `docs/Implementation_Plan.md`.

Use:

- Clean Architecture
- Feature-oriented organization
- Vertical Slice Architecture
- Thin Controllers
- DTOs
- Dependency Injection
- Async/Await
- RESTful APIs

Do not introduce architectural changes without approval.

## Documentation Rules

If implementation changes require updates to the architecture or database design, update the corresponding documentation before merging.

The order of authority is:

1. `docs/Project_00_Master_Document.md`
2. `docs/Database_Specification.md`
3. `docs/Implementation_Plan.md`
4. Figma designs

## Code Quality

Prefer:

- Readable code
- Small methods
- Meaningful names
- SOLID principles
- Composition over inheritance
- Minimal duplication

Avoid unnecessary complexity.

## Final Notes

This repository is developed incrementally. Complete one vertical slice at a time, and prioritize maintainability over premature optimization.
