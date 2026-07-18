"use client";

import React, { useState } from "react";
import CoefficientSummaryPanel from "./CoefficientSummaryPanel";
import UnitsList from "./UnitsList";

export default function UnitsPageContent() {
  const [coefficientRefreshTrigger, setCoefficientRefreshTrigger] = useState(0);

  const handleUnitsChanged = () => {
    setCoefficientRefreshTrigger((previous) => previous + 1);
  };

  return (
    <>
      <CoefficientSummaryPanel refreshTrigger={coefficientRefreshTrigger} />
      <UnitsList onUnitsChanged={handleUnitsChanged} />
    </>
  );
}
