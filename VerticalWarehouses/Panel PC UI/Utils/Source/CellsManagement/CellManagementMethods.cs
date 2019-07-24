﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Ferretto.VW.Utils.Source.CellsManagement
{
    public static class CellManagementMethods
    {
        #region Fields

        public const int AISLE_SIDES_COUNT = 2;

        public const int CELL_HEIGHT_MILLIMETERS = 25;

        public static readonly string JSON_BLOCK_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["BlocksFilePath"]);

        public static readonly string JSON_CELL_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["CellsFilePath"]);

        #endregion

        #region Methods

        public static void CompactCells(CellsManager cm)
        {
            throw new NotImplementedException();
        }

        public static void CompactCells(CellsManager cm, CellSide side)
        {
            throw new NotImplementedException();
        }

        public static void CompactCells(CellsManager cm, int heightMillimiters)
        {
            throw new NotImplementedException();
        }

        public static bool CreateBay(CellsManager cm, int firstCellID, int lastCellID)
        {
            if ((firstCellID % 2 == 0 && lastCellID % 2 != 0) || (firstCellID % 2 != 0 && lastCellID % 2 == 0))
            {
                return false;
            }

            if (cm.BayCounter > 3)
            {
                return false;
            }

            for (var id = firstCellID; id <= lastCellID; id += 2)
            {
                ChangeCellStatus(cm, id - 1, CellStatus.Disabled);
            }

            cm.Bays.Add(new Bay(++cm.BayCounter, ((lastCellID - firstCellID) / AISLE_SIDES_COUNT) * CELL_HEIGHT_MILLIMETERS, firstCellID));
            return true;
        }

        public static bool CreateBlocks(CellsManager cm)
        {
            cm.Blocks = null;
            cm.Blocks = new List<CellBlock>();
            var counter = 1;
            for (var index = 0; index < cm.Cells.Count; index += 2) //odd ID cell's index
            {
                if (cm.Cells[index].Status == 0)
                {
                    var tmp = GetLastUpperNotDisabledCellIndex(cm, index);
                    var cb = new CellBlock(index + 1, tmp + 1, counter);
                    cm.Blocks.Add(cb);
                    counter++;
                    index = tmp;
                }
            }

            for (var index = 1; index < cm.Cells.Count; index += 2)//even ID cell's index
            {
                if (cm.Cells[index].Status == 0)
                {
                    var tmp = GetLastUpperNotDisabledCellIndex(cm, index);
                    var cb = new CellBlock(index + 1, tmp + 1, counter);
                    cm.Blocks.Add(cb);
                    counter++;
                    index = tmp;
                }
            }

            UpdateBlocksFile(cm);
            return true;
        }

        public static void CreateCellTable(CellsManager cm, int machineHeightMillimiters)
        {
            var cells = CalculateCellQuantityFromMachineHeight(machineHeightMillimiters);
            for (var id = 1; id <= cells; id++)
            {
                var c = new Cell(id);
                cm.Cells.Add(c);
            }

            UpdateCellsFile(cm);
        }

        public static bool ExtractDrawer(CellsManager cm, int drawerID, int destinationBayID)
        {
            if (cm.Bays[destinationBayID - 1].Occupied)
            {
                return false;
            }

            if (cm.Drawers.Find(x => x.Id == drawerID) == null)
            {
                return false;
            }

            cm.Bays[destinationBayID - 1].Occupied = true;
            cm.Bays[destinationBayID - 1].DrawerID = drawerID;
            var d = (from ret_d in cm.Drawers where ret_d.Id == drawerID select ret_d).First();
            FreeCells(cm, d.FirstCellID - 1, d.HeightMillimiters);
            UpdateCellsFile(cm);
            CreateBlocks(cm);
            UpdateBlocksFile(cm);
            return true;
        }

        public static int GetFreeCellQuantity(CellsManager cm)
        {
            return cm.Blocks.Sum(x => x.FinalIDCell - x.InitialIDCell);
        }

        public static int GetFreeCellQuantity(CellsManager cm, CellSide side)
        {
            return cm.Blocks.Where(x => x.Side == side).Sum(x => x.FinalIDCell - x.InitialIDCell);
        }

        public static void InsertBays(CellsManager cm)
        {
            CreateBlocks(cm);
            UpdateCellsFile(cm);
            UpdateBlocksFile(cm);
        }

        public static bool InsertNewDrawer(CellsManager cm, int newDrawerID, int newDrawerHeightMillimiters)
        {
            var initCellID = FindFirstFreeCellIDForDrawerInsert(cm, newDrawerHeightMillimiters);
            if (initCellID < 0)
            {
                return false;
            }
            var d = new Drawer(newDrawerID, newDrawerHeightMillimiters, initCellID);
            cm.Drawers.Add(d);
            OccupyCells(cm, initCellID - 1, newDrawerHeightMillimiters);
            UpdateCellsFile(cm);
            CreateBlocks(cm);
            UpdateBlocksFile(cm);
            return true;
        }

        public static bool InsertUnusableCell(CellsManager cm, int cellID)
        {
            if (cm.Cells[cellID - 1].Status != CellStatus.Free)
            {
                return false;
            }
            ChangeCellStatus(cm, cellID - 1, CellStatus.Unusable);
            CreateBlocks(cm);
            UpdateCellsFile(cm);
            UpdateBlocksFile(cm);
            return true;
        }

        public static bool ReInsertDrawer(CellsManager cm, int bayID)
        {
            if (cm.Bays.Count < bayID || !cm.Bays[bayID - 1].Occupied)
            {
                return false;
            }
            InsertNewDrawer(cm, cm.Bays[(int)bayID - 1].DrawerID, cm.Drawers[cm.Bays[bayID - 1].DrawerID - 1].HeightMillimiters);
            cm.Bays[bayID - 1].Occupied = false;
            cm.Bays[bayID - 1].DrawerID = -1;
            CreateBlocks(cm);
            UpdateCellsFile(cm);
            UpdateBlocksFile(cm);
            return true;
        }

        public static void UpdateBlocksFile(CellsManager cm)
        {
            var json = JsonConvert.SerializeObject(cm.Blocks, Formatting.Indented);

            if (File.Exists(JSON_BLOCK_PATH))
            {
                File.Delete(JSON_BLOCK_PATH);
            }
            File.WriteAllText(JSON_BLOCK_PATH, json);
        }

        public static void UpdateCellsFile(CellsManager cm)
        {
            var json = JsonConvert.SerializeObject(cm.Cells, Formatting.Indented);

            if (File.Exists(JSON_CELL_PATH))
            {
                File.Delete(JSON_CELL_PATH);
            }
            File.WriteAllText(JSON_CELL_PATH, json);
        }

        private static int CalculateCellQuantityFromMachineHeight(int machineHeightMillimiters)
        {
            var cells = machineHeightMillimiters / CELL_HEIGHT_MILLIMETERS;
            return cells * AISLE_SIDES_COUNT;
        }

        private static void ChangeCellStatus(CellsManager cm, int cellIndex, CellStatus newStatus)
        {
            if (cellIndex >= cm.Cells.Count || cellIndex < 0)
            {
                throw new ArgumentException("CellsManagement Exception: cellID does not point to any cell in memory.", nameof(cellIndex));
            }
            cm.Cells[cellIndex].Status = newStatus;
        }

        private static int FindFirstFreeCellIDForDrawerInsert(CellsManager cm, int drawerHeightMillimiters)
        {
            var tempCellBlock = cm.Blocks.Where(x => x.BlockHeightMillimiters > drawerHeightMillimiters).OrderBy(x => x.Priority).ToList();

            if (tempCellBlock.Count > 0)
            {
                return tempCellBlock[0].InitialIDCell;
            }
            else
            {
                return -1;
            }
        }

        private static void FreeCells(CellsManager cm, int firstCellIndex, int drawerHeightMillimiters)
        {
            var cellsToFree = drawerHeightMillimiters / CELL_HEIGHT_MILLIMETERS;
            for (var index = firstCellIndex; index <= firstCellIndex + (cellsToFree * AISLE_SIDES_COUNT); index += 2)
            {
                ChangeCellStatus(cm, index, 0);
            }
        }

        private static int GetLastUpperNotDisabledCellIndex(CellsManager cm, int cellIndex)
        {
            for (var index = cellIndex + 2; index < cm.Cells.Count; index += 2)
            {
                if (cm.Cells[index].Status != CellStatus.Disabled
                    &&
                    cm.Cells[index].Status != CellStatus.Occupied)
                {
                }
                else
                {
                    return index - 2;
                }
            }
            return (cellIndex % 2 == 0) ? cm.Cells.Count - 2 : cm.Cells.Count - 1;
        }

        private static void OccupyCells(CellsManager cm, int firstCellindex, int drawerHeightMillimiters)
        {
            var cellsToOccupy = (drawerHeightMillimiters / CELL_HEIGHT_MILLIMETERS) + 1;
            for (var index = firstCellindex; index <= firstCellindex + (cellsToOccupy * AISLE_SIDES_COUNT); index += 2)
            {
                ChangeCellStatus(cm, index, CellStatus.Occupied);
            }
        }

        #endregion
    }
}
