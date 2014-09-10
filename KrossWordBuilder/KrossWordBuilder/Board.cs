using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KrossWordBuilder
{
    public class Board
    {
        public Board(int size)
        {
            Grids = new string[size, size];
            CellBoard = new Cell[size, size];
            RowSize = size;
        }

        public string[,] Grids { get; set; }
        public Cell[,] CellBoard { get; set; }
        public int RowSize { get; set; }

        private bool IsEmpty()
        {
            return Grids.Length > 0 & string.IsNullOrEmpty(Grids[0, 0]);
        }

        public bool AddWord(string word)
        {
            string[] wordArray = word.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

            if (IsEmpty())
            {
                AddFirstWord(wordArray);
                return true;
            }

            int row = 0;
            bool matchFound = false;
            do
            {
                for (int col = 0; col < RowSize; col++)
                {
                    InsertWordResult insertWordResult = AttemptToAddWordVertically(wordArray, row, col);
                    matchFound = insertWordResult.Inserted;
                    if (matchFound) break;
                }
                row += 1;
            } while (row + wordArray.Length <= RowSize && matchFound == false);

            return matchFound;
        }

        private InsertWordResult AttemptToAddWordVertically(string[] word, int currentRow, int col)
        {
            int wordLength = word.Length;

            //Add only if first cell has been prepopulated with first word; 
            if (string.IsNullOrEmpty(Grids[currentRow, col])) return new InsertWordResult();
            InsertWordResult wordInsertedResult = CanWordBeAddedVerticallyToRow(wordLength, currentRow, col, word);
            if (wordInsertedResult.Inserted)
            {
                AddWordVerticallyToRow(wordLength, currentRow, col, word);
            }
            return wordInsertedResult;
        }

        private InsertWordResult CanWordBeAddedVerticallyToRow(int wordLength, int row, int currentCol, string[] word)
        {
            bool letterMatch = false;
            bool letterMismatch = false;
            int currentRow = row;
            for (int i = 0; i < wordLength; i++)
            {
                if (Grids[currentRow, currentCol] == word[i])
                {
                    letterMatch = true;
                }
                else if (Grids[currentRow, currentCol] != word[i] &&
                         string.IsNullOrEmpty(Grids[currentRow, currentCol]) == false)
                {
                    //The cell is not empty and there is a mismatch in the cell for the letter and 
                    letterMismatch = true;
                    break;
                }
                currentRow += 1;
            }

            var insertResult = new InsertWordResult
            {
                Inserted = letterMatch && !letterMismatch,
                StartCell = Tuple.Create(row, currentCol),
                EndCell = Tuple.Create(currentRow, currentCol),
                Word = word
            };

            insertResult.Inserted = ValidateSuffixRule(insertResult);
            return insertResult;
        }

        private void AddWordVerticallyToRow(int wordLength, int row, int currentCol, string[] word)
        {
            int currentRow = row;
            for (int i = currentRow; i < wordLength; i++)
            {
                var cell = new Cell
                {
                    Character = word[i],
                    Col = currentCol,
                    Row = currentRow,
                    WordV = word,
                    IndexV = currentRow
                };

                if (currentRow == row)
                {
                    cell.IsFirstLetter = true;
                }
                else
                {
                    cell.VerticalPreceedingRelative = Tuple.Create(currentRow - 1, currentCol);
                }
                //If letter is already on the board for another word, ignore.
                if (Grids[currentRow, currentCol] != word[i])
                {
                    Grids[currentRow, currentCol] = word[i];
                    CellBoard[currentRow, currentCol] = cell;
                }
                else
                {
                    CellBoard[currentRow, currentCol].IsFirstLetter = true;
                    CellBoard[currentRow, currentCol].IsJunction = true;
                }

                currentRow += 1;
            }
        }

        private void AddFirstWord(string[] wordArray)
        {
            //First word is added horizontally
            for (int i = 0; i < wordArray.Length; i++)
            {
                var cell = new Cell
                {
                    Character = wordArray[i],
                    Row = 0,
                    Col = i,
                    WordH = wordArray,
                    Index = i
                };

                if (i == 0)
                {
                    cell.IsFirstLetter = true;
                }
                else
                {
                    cell.HorizontalPreceedingRelative = Tuple.Create(0, i - 1);
                }
                CellBoard[0, i] = cell;
                Grids[0, i] = wordArray[i];
            }
        }

        private bool ValidateSuffixRule(InsertWordResult insertWordResult)
        {
            if (!insertWordResult.Inserted) return false;
            int nextrow = insertWordResult.EndCell.Item1;
            int col = insertWordResult.EndCell.Item2;
            return string.IsNullOrEmpty(Grids[nextrow, col]);
        }

        public IEnumerable<Cell> GetLoadedCells2()
        {
            var result = new List<Cell>();
            for (int i = 0; i < Grids.GetLength(0); i++)
            {
                for (int j = 0; j < Grids.GetLength(1); j++)
                {
                    if (! string.IsNullOrEmpty(Grids[i, j]))
                    {
                        result.Add(new Cell
                        {
                            Character = Grids[i, j],
                            Col = j,
                            Row = i
                        });
                    }
                }
            }
            return result;
        }

        public IEnumerable<Cell> GetLoadedCells()
        {
            return BoardCellsWithValues().Where(x => string.IsNullOrEmpty(x.Character) == false);
        }

        public IEnumerable<Cell> BoardCellsWithValues()
        {
            for (int i = 0; i < Grids.GetLength(0); i++)
            {
                for (int j = 0; j < Grids.GetLength(1); j++)
                {
                    if (CellBoard[i, j] != null)
                    {
                        yield return CellBoard[i, j];
                    }
                }
            }
        }

        public IEnumerable<Cell> GetLetterMatchesFor(string wordToInsert)
        {
            return BoardCellsWithValues().Where(x => wordToInsert.Contains(x.Character));
        }

        public bool IsCellVerticallyOccupied(Cell cell)
        {
            //? Cell ==> First
            //? Cell ==> Junction
            //? Cell ==> A continuous array of chars -x and -y

            return cell.WordV != null;
        }

        private bool IsCellAtPosEmpty(int row, int col)
        {
            return CellBoard[row, col] == null || string.IsNullOrEmpty(CellBoard[row, col].Character);
        }

        public void AddHorizontally(string wordToInsert)
        {
            IEnumerable<Cell> matchedCellVertically =
                BoardCellsWithValues().Where(x => wordToInsert.Contains(x.Character) && x.WordV != null);
            string[] wordArray = wordToInsert.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

            foreach (Cell cell in matchedCellVertically)
            {
                if (CanWordBeAddedFromCellPosVertically(cell, wordArray))
                {
                    InsertWordHorizontally(cell, wordArray);
                }
            }
        }

        private void InsertWordHorizontally(Cell cell, string[] wordArray)
        {
            throw new NotImplementedException();
        }

        private bool CanWordBeAddedFromCellPosVertically(Cell cell, string[] wordArray)
        {
            bool gridMatchFound = false;
            Cell cell1 = cell;
            int matchIndex = Array.FindIndex(wordArray, x => x == cell1.Character);
            while (matchIndex > -1 && gridMatchFound == false)
            {
                bool possible = true;
                for (int i = 0; i < wordArray.Length; i++)
                {
                    string horCell = CellBoard[cell.Row, cell.Col - matchIndex].Character;
                    if (!string.IsNullOrEmpty(horCell) || horCell != wordArray[i])
                    {
                        possible = false;
                        break;
                    }
                }

                matchIndex = Array.FindIndex(wordArray, matchIndex + 1, x => x == cell1.Character);
            }
            return possible;
        }
    }
}