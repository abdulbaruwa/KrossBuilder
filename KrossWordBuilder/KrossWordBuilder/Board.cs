using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KrossWordBuilder
{
    public static class IntergerExtensions
    {
        public static bool IntegerWithin(this int val, int lower, int upper)
        {
            if (val >= lower && val <= upper) return true;
            return false;
        }
    }

    public enum AcrosVertical
    {
        Across,
        Vertical
    };

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

        public bool AddWord2(string word)
        {
            string[] wordArray = word.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
            if (wordArray.Length > RowSize) return false;
            if (IsEmpty())
            {
                AddFirstWord(wordArray);
                return true;
            }

            var vertAttempt = AddVertically(word);
            var horAttempt = new Tuple<bool, int>(false,-1);
            if (vertAttempt.Item1 == false)
            {
                horAttempt = AddHorizontally(word);
            }

            return vertAttempt.Item1 ? vertAttempt.Item1 : horAttempt.Item1;
        }

        public List<string> ProcessWords(string[] words)
        {
            var failed = new List<string>();
            foreach (var word in words)
            {
                Console.WriteLine("Inserting word '{0}', length = {1} ", word, word.Length);
                if (! AddWord2(word)){failed.Add(word);}
                PrintBoard(this);
            }

            return failed;
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
                    IndexH = i
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

        public Tuple<bool, int> AddHorizontally(string wordToInsert)
        {
            IEnumerable<Cell> matchedCellVertically =
                BoardCellsWithValues().Where(x => wordToInsert.Contains(x.Character) && x.WordV != null);
            string[] wordArray = wordToInsert.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

            foreach (Cell cell in matchedCellVertically)
            {
                var canWordBeAddedFromCellPosVerticallyResult = CanWordBeAddedFromCellPosHorizontally(cell, wordArray);
                if (canWordBeAddedFromCellPosVerticallyResult.Item1 && ValidateAround(cell, canWordBeAddedFromCellPosVerticallyResult.Item2, wordArray, AcrosVertical.Across))
                {
                    InsertWordHorizontally(cell, canWordBeAddedFromCellPosVerticallyResult.Item2, wordArray);
                    return canWordBeAddedFromCellPosVerticallyResult;
                }
            }
            return Tuple.Create(false, -1);
        }

        public Tuple<bool, int> AddVertically(string wordToInsert)
        {
            IEnumerable<Cell> matchedHorizontally =
                BoardCellsWithValues().Where(x => wordToInsert.Contains(x.Character) && x.WordH != null);
            string[] wordArray = wordToInsert.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

            foreach (Cell cell in matchedHorizontally)
            {
                var canWordBeAddedFromCellPosVerticallyResult = CanWordBeAddedFromCellPosVertically(cell, wordArray);
                if (canWordBeAddedFromCellPosVerticallyResult.Item1 && ValidateAround(cell, canWordBeAddedFromCellPosVerticallyResult.Item2, wordArray, AcrosVertical.Vertical))
                {
                    InsertWordVertically(cell, canWordBeAddedFromCellPosVerticallyResult.Item2, wordArray);
                    return canWordBeAddedFromCellPosVerticallyResult;
                }
            }
            return Tuple.Create(false, -1);
        }

        /// <summary>
        /// Validate that Prefix, Suffix cells of word do not contain a letter 
        /// And that cells above and below (for horizontal word) or right and left (for vertical word) are also empty.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="indexInWordArray"></param>
        /// <param name="wordArray"></param>
        /// <param name="isVertical"></param>
        /// <returns></returns>
        private bool ValidateAround(Cell cell, int indexInWordArray, string[] wordArray, AcrosVertical isVertical)
        {
            if (isVertical == AcrosVertical.Vertical)
            {
                return ValidateAroundForVertical(cell, indexInWordArray, wordArray);
            }
            return ValdateAroundForHorizontal(cell, indexInWordArray, wordArray);

        }

        private bool ValidateAroundForVertical(Cell cell, int indexInWordArray, string[] wordArray)
        {
            var startPosVert = cell.Row - indexInWordArray;
            var endPosHor = startPosVert + wordArray.Length -1;

            if (endPosHor > RowSize - 1) return false;

            //check cells above and below each word cell
            for (var i = 0; i < wordArray.Length; i++)
            {
                if (i == indexInWordArray) continue;
                if (CellBoard[startPosVert + i, cell.Col + 1] != null || CellBoard[startPosVert + i, cell.Col - 1] != null)
                {
                    return false;
                }
            }

            //Check prefix and suffix cells
            bool prefixOk = false;
            bool suffixOk = false;
            if (startPosVert == 0) prefixOk = true;
            if (endPosHor == 0) suffixOk = true;
            if (startPosVert > 0 && CellBoard[startPosVert - 1, cell.Col] == null)
            {
                prefixOk = true;
            }

            if ((endPosHor == RowSize-1) || endPosHor > 0 && CellBoard[endPosHor + 1, cell.Col] == null)
            {
                suffixOk = true;
            }
            return prefixOk && suffixOk; // CellBoard[startPosVert - 1, cell.Col] == null && CellBoard[endPosHor + 1, cell.Col] == null;
        }

        private bool ValdateAroundForHorizontal(Cell cell, int indexInWordArray, string[] wordArray)
        {
            var startPosHori = cell.Col - indexInWordArray;
            var endPos = startPosHori + wordArray.Length - 1;

            //check cells above and below each word cell, but ignore checking the around the match position
            if (cell.Row - 1 < 0) return false;
            if (endPos > RowSize - 1) return false;

            for (var i = 0; i < wordArray.Length; i++)
            {
                if (i == indexInWordArray) continue;

                // Ignore check below cell if on last row
                if (cell.Row == RowSize)
                {
                    if (CellBoard[cell.Row - 1, startPosHori + i] != null)
                    {
                        return false;
                    } 
                }
                else if (cell.Row == 0) //Ignore check above cell if on first row
                {
                    if (CellBoard[cell.Row + 1, startPosHori + i] != null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (CellBoard[cell.Row + 1, startPosHori + i] != null ||
                        CellBoard[cell.Row - 1, startPosHori + i] != null)
                    {
                        return false;
                    }
                    
                }
            }

            bool prefixOk = false;
            bool suffixOk = false;
            if (startPosHori == 0) prefixOk = true;
            if (endPos == 0) suffixOk = true;

            if (startPosHori > 0 && CellBoard[cell.Row, startPosHori - 1] == null)
            {
                prefixOk = true;
            }
            if ((endPos == RowSize - 1) || endPos > 0 &&  CellBoard[cell.Row, endPos + 1] == null)
            {
                suffixOk = true;
            }

            //Check prefix and suffix cells
            return prefixOk && suffixOk; // CellBoard[cell.Row, startPosHori - 1] == null && CellBoard[cell.Row, endPos + 1] == null;
        }

        /// <summary>
        ///  Insert Word Horizontally, Called internally after it has been determined a word can be inserted.
        /// </summary>
        /// <param name="cell">Cell Position in Grid where match is found</param>
        /// <param name="matchIndexInword">IndexH position in word array to be inserted where a match on the grid has been found</param>
        /// <param name="wordArray">Word array to be inserted</param>
        private void InsertWordHorizontally(Cell cell1, int matchIndexInword, string[] wordArray)
        {
            var startCol = cell1.Col - matchIndexInword;
            var currentRow = cell1.Row;
            int currentCol = startCol;
            for (int i = 0; i < wordArray.Length; i++)
            {
                var cell = new Cell
                {
                    Character = wordArray[i],
                    Col = currentCol,
                    Row = cell1.Row,
                    WordH = wordArray,
                    IndexH = currentCol
                };

                if (currentCol == startCol)
                {
                    cell.IsFirstLetter = true;
                }
                else
                {
                    cell.HorizontalPreceedingRelative = Tuple.Create(cell1.Row, currentCol-1);
                }
                //If letter is already on the board for another word, ignore.
                if (Grids[currentRow, currentCol] != wordArray[i])
                {
                    Grids[currentRow, currentCol] = wordArray[i];
                    CellBoard[currentRow, currentCol] = cell;
                }
                else
                {
                    CellBoard[currentRow, currentCol].IsFirstLetter = true;
                    CellBoard[currentRow, currentCol].IsJunction = true;
                }

                currentCol += 1;
            }
        }

        private void InsertWordVertically(Cell cell1, int matchIndexInword, string[] wordArray)
        {
            var startRow = cell1.Row - matchIndexInword;
            var startCol = cell1.Col;
            var currentRow = startRow;
            int currentCol = startCol;
            for (int i = 0; i < wordArray.Length; i++)
            {
                var cell = new Cell
                {
                    Character = wordArray[i],
                    Col = cell1.Col,
                    Row = currentRow,
                    WordV = wordArray,
                    IndexV = currentRow
                };

                if (currentCol == startRow)
                {
                    cell.IsFirstLetter = true;
                }
                else
                {
                    cell.HorizontalPreceedingRelative = Tuple.Create(cell1.Row -1, currentCol);
                }
                //If letter is already on the board for another word, ignore.
                if (Grids[currentRow, currentCol] != wordArray[i])
                {
                    Grids[currentRow, currentCol] = wordArray[i];
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

        private Tuple<bool, int> CanWordBeAddedFromCellPosHorizontally(Cell cell, string[] wordArray)
        {
            bool gridMatchFound = false;
            Cell cell1 = cell;
            int matchIndex = Array.FindIndex(wordArray, x => x == cell1.Character);
            int lastIndex = -1;

            //for each occurence of the matching letter in the word.
            while (matchIndex > -1 && gridMatchFound == false)
            {
                lastIndex = matchIndex;
                gridMatchFound = true;
                // for each letter in word
                for (int i = 0; i < wordArray.Length; i++)
                {
                    if (cell.Col - matchIndex < 0)
                    {
                        gridMatchFound = false;
                        break;
                    }

                    var colPos = cell.Col - matchIndex + i;

                    if ((!colPos.IntegerWithin(0, 11)) || CellBoard[cell.Row, colPos] != null && CellBoard[cell.Row, colPos].Character != wordArray[i])
                    {
                        gridMatchFound = false;
                        break;
                    }
                }
                matchIndex = Array.FindIndex(wordArray, matchIndex + 1, x => x == cell1.Character);
            }

            return  Tuple.Create(gridMatchFound, lastIndex);
        }

        private Tuple<bool, int> CanWordBeAddedFromCellPosVertically(Cell cell, string[] wordArray)
        {
            bool gridMatchFound = false;
            Cell cell1 = cell;
            int matchIndex = Array.FindIndex(wordArray, x => x == cell1.Character);
            int lastIndex = -1;

            //for each occurence of the matching letter in the word.
            while (matchIndex > -1 && gridMatchFound == false)
            {
                lastIndex = matchIndex;
                gridMatchFound = true;
                // for each letter in word
                for (int i = 0; i < wordArray.Length; i++)
                {
                    if (cell.Col - matchIndex < 0)
                    {
                        gridMatchFound = false;
                        break;
                    }

                    var rowPos = cell.Row - matchIndex + i;

                    if ( (!rowPos.IntegerWithin(0, 11)) || (CellBoard[rowPos, cell.Col] != null && CellBoard[rowPos, cell.Col].Character != wordArray[i]))
                    {
                        gridMatchFound = false;
                        break;
                    }
                }
                matchIndex = Array.FindIndex(wordArray, matchIndex + 1, x => x == cell1.Character);
            }

            return Tuple.Create(gridMatchFound, lastIndex);
        }

        private void PrintBoard(Board board)
        {
            for (int i = 0; i < board.CellBoard.GetLength(0); i++)
            {
                string row = "";
                for (int j = 0; j < board.CellBoard.GetLength(1); j++)
                {
                    if (board.CellBoard[i, j] == null)
                    {
                        row = row + " " + "-";
                    }
                    else
                    {
                        row = row + " " + board.CellBoard[i, j].Character;
                    }
                }
                Console.WriteLine(row);
            }
        }
    }
}
