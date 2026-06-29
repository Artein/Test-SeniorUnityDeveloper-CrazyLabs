## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Extract the current pickup balance/grant path into the target **Economy** glossary and add the spend capability needed by upgrades. The completed slice should let gameplay grant coin currency from pickups, read app-session balances, spend currency atomically through `TrySpend`, and keep per-run earned totals without using `Resource` as the gameplay economy domain term.

This slice should preserve the existing pickup collection behavior while changing the domain language and storage contract to support future upgrade purchases.

## Acceptance criteria

- [ ] `CurrencyDefinition` is the asset-backed identity for spendable/grantable gameplay currency, initially coins.
- [ ] `CurrencyGrant` represents one positive currency amount from a pickup or gameplay source.
- [ ] `CurrencyStorage` / `ICurrencyStorage` supports `GetAmount`, `Grant`, and `TrySpend` with app-session lifetime.
- [ ] `TrySpend` returns false only for insufficient balance; invalid currency or amount inputs fail fast.
- [ ] `RunCurrencyAccumulator` and `RunCurrencySnapshot` record current-run final earned currency totals and can reset between runs.
- [ ] Existing pickup definitions grant currency through `CurrencyGrant` while remaining separate from currency definitions.
- [ ] Pickup collection continues to grant current coin pickup amounts to app-session storage and run accumulation with x1 behavior.
- [ ] Gameplay economy runtime code does not introduce `Resource` or `Resources` as a domain, namespace, assembly, folder, or class name.
- [ ] Existing gameplay composition uses the Economy storage contracts rather than the previous resource storage contract.

## Verification

- EditMode tests:
  - Currency storage grants and queries balances by currency definition.
  - Currency storage returns zero for missing balances.
  - Currency storage spends exact balances and rejects insufficient spends without mutation.
  - Currency storage fails fast for invalid definitions or invalid amounts.
  - Run currency accumulator accumulates, resets, and creates immutable sparse snapshots.
  - Pickup definition validation rejects missing currency grants and non-positive grant amounts.
- PlayMode tests:
  - Existing pickup collection smoke path still grants coin currency and updates run accumulation.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Search gameplay runtime code for leftover economy-domain `Resource` names, excluding historical docs or unrelated Unity APIs.
- Manual Unity smoke check:
  - Collect a regular and big coin pickup and confirm app-session balance and run-earned totals increase by the authored amounts.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
