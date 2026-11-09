# South African Crash Data — Validation Status

**Dataset:** Cape Town traffic accident records, 2019–2024
**Status date:** 7 November 2026

## Cleaning summary

| Measure | Result |
|---|---:|
| Original records | 377,996 |
| Duplicate-labelled records excluded | 4,355 |
| Incident-report records excluded | 3,955 |
| Total excluded | 8,371 |
| Cleaned crash records | 369,625 |
| Missing `CRASH_TYPE` after cleaning | 0 |

The exclusions were made using the exact `ROAD_DESCRIPTION` placeholders `DUPLICATE ACCIDENT - DO NOT USE` and `INCIDENT REPORT`. They were not based solely on missing `CRASH_TYPE`.

## Latest road-category severity results

| Derived road category | Crashes | Fatalities (people) | Serious injuries (people) | Fatal-crash rate | Serious-crash rate |
|---|---:|---:|---:|---:|---:|
| National Route | 27,012 | 310 | 1,046 | 1.040% | 3.143% |
| Regional Route | 36,468 | 262 | 1,370 | 0.644% | 3.126% |
| Metropolitan Route | 120,346 | 518 | 4,407 | 0.392% | 3.220% |
| Other / Unclassified | 185,799 | 407 | 6,494 | 0.210% | 3.159% |

Fatal-crash rate means the proportion of crash records with at least one fatality (`FATAL > 0`). Serious-crash rate means the proportion with at least one serious injury (`SERIOUS > 0`). Fatalities and serious injuries are person counts, not crash counts.

## Interpretation and limits

- National routes show the highest fatal-crash rate in this dataset. This is descriptive association, not evidence that route class causes fatal crashes.
- Road class was inferred from route-number patterns in `ROAD_DESCRIPTION`; it is not an authoritative road inventory. `Other / Unclassified` is heterogeneous.
- Counts and crash-level rates are not adjusted for traffic volume, distance travelled, or road-user exposure.
- The data is Cape Town-focused and should not be described as nationally representative of all South African roads.
- The Cape Town dataset does not supply the full 12-feature input set or the model target `accident_risk`; it cannot directly establish predictive accuracy for the Kaggle-trained model.
- Geographic/temporal validation requires reliable segment matching, feature sources, target definition, and separation of validation locations or time periods.

## Recommended next validation work

1. Confirm data provenance, extraction coverage, reporting gaps, and licensing conditions.
2. Source road-segment attributes such as speed limit, lane count, curvature, lighting, signage, and road class.
3. Define a historical look-back window and point-in-time availability for `num_reported_accidents` to prevent future-information leakage.
4. Verify the original training data's speed-limit unit from authoritative metadata before using any km/h conversion.
5. Design geographically and/or temporally separated validation with a defensible outcome label.
