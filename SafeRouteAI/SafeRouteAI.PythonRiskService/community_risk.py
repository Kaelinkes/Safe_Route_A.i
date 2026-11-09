"""Transparent, separate scoring for approved community hazard reports.

This score is deliberately NOT described as ML-trained. The default blending
weight is conservative and configurable; it requires project-owner calibration.
"""
from __future__ import annotations
from datetime import datetime, timezone
from math import exp

SEVERITY = {"low": 0.25, "medium": 0.55, "high": 0.85, "critical": 1.0}
DEFAULT_HALF_LIFE_HOURS = 72.0
DEFAULT_RADIUS_METRES = 500.0


def _parse_time(value):
    if isinstance(value, datetime):
        dt = value
    elif isinstance(value, str):
        dt = datetime.fromisoformat(value.replace("Z", "+00:00"))
    else:
        return None
    if dt.tzinfo is None:
        dt = dt.replace(tzinfo=timezone.utc)
    return dt.astimezone(timezone.utc)


def score_approved_reports(reports, now=None, half_life_hours=DEFAULT_HALF_LIFE_HOURS):
    """Return 0..1 hazard score and auditable explanation; ignore unapproved reports.

    Optional report fields: approved, severity, reported_at, distance_m. Reports
    without an approval flag, timestamp, known severity or valid distance are ignored.
    """
    now = _parse_time(now) if now is not None else datetime.now(timezone.utc)
    contributions, explanations = [], []
    for report in reports or []:
        if not isinstance(report, dict) or report.get("approved") is not True:
            continue
        severity = str(report.get("severity", "")).lower()
        reported_at = _parse_time(report.get("reported_at"))
        try:
            distance = float(report.get("distance_m", 0))
        except (TypeError, ValueError):
            continue
        if severity not in SEVERITY or reported_at is None or distance < 0 or distance > DEFAULT_RADIUS_METRES:
            continue
        age_hours = max(0.0, (now - reported_at).total_seconds() / 3600.0)
        recency = exp(-0.69314718056 * age_hours / max(float(half_life_hours), 1e-6))
        proximity = max(0.0, 1.0 - distance / DEFAULT_RADIUS_METRES)
        contribution = SEVERITY[severity] * recency * proximity
        if contribution > 0:
            contributions.append(contribution)
            explanations.append({"type": str(report.get("type", "community hazard")),
                                 "severity": severity, "distanceMetres": round(distance),
                                 "ageHours": round(age_hours, 1), "weightedScore": round(contribution, 4)})
    # Saturating aggregation: multiple reports increase score without unbounded growth.
    combined = 1.0
    for c in contributions:
        combined *= (1.0 - min(c, 1.0))
    return {"score": round(1.0 - combined, 4), "approvedReportsUsed": len(contributions),
            "explanations": explanations}


def blend_risk(ml_score, community_score, alpha=0.15):
    """Apply a non-negative hazard uplift; alpha needs local calibration.

    No reports (None) leaves the ML score unchanged. Approved community hazards
    can increase, but never reduce, the ML risk estimate.
    """
    alpha = float(alpha)
    if not 0 <= alpha <= 1:
        raise ValueError("community blend alpha must be between 0 and 1")
    base = float(ml_score)
    if community_score is None:
        return base
    hazard = min(1.0, max(0.0, float(community_score)))
    return base + alpha * hazard * (1.0 - base)
