CREATE TABLE "InstructionDefinitions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InstructionDefinitions" PRIMARY KEY AUTOINCREMENT,
    "Axis" INTEGER NOT NULL DEFAULT 0,
    "InstructionType" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "CounterName" TEXT NULL,
    "MaxDays" INTEGER NULL,
    "MaxRelativeCount" INTEGER NULL,
    "MaxTotalCount" INTEGER NULL,
    "BayNumber" INTEGER NOT NULL,
    "IsShutter" INTEGER NOT NULL,
    "IsCarousel" INTEGER NOT NULL,
    "IsSystem" INTEGER NOT NULL
);


CREATE TABLE "Instructions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Instructions" PRIMARY KEY AUTOINCREMENT,
    "DefinitionId" INTEGER NULL,
    "DoubleCounter" REAL NULL,
    "IntCounter" INTEGER NULL,
    "IsDone" INTEGER NOT NULL,
    "IsToDo" INTEGER NOT NULL,
    "MaintenanceDate" TEXT NULL,
    "ServicingInfoId" INTEGER NULL,
    CONSTRAINT "FK_Instructions_InstructionDefinitions_DefinitionId" FOREIGN KEY ("DefinitionId") REFERENCES "InstructionDefinitions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Instructions_ServicingInfo_ServicingInfoId" FOREIGN KEY ("ServicingInfoId") REFERENCES "ServicingInfo" ("Id") ON DELETE RESTRICT
);

update servicingInfo set MachineStatisticsId = -1;

alter table MovementParameters add column AdjustAccelerationByWeight INTEGER;
alter table MovementParameters add column AdjustSpeedByWeight INTEGER;

update MovementParameters set AdjustSpeedByWeight = AdjustByWeight;
update MovementParameters set AdjustAccelerationByWeight = AdjustByWeight;


