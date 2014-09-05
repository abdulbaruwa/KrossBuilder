using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace CrossWordBuilder
{
    public class InsertWordResult
    {
        public bool IsVertical{get;set;}
        public Tuple<int, int> StartCell{get;set;}
        public Tuple<int, int> EndCell{get;set;}
        public bool Inserted { get; set; }
        public string[] Word { get; set; }
    }

    public class Board
    {
        public string[,] Grids { get; set; }
        public int RowSize { get; set; }
        private bool IsEmpty()
        {
            return Grids.Length > 0 & string.IsNullOrEmpty(Grids[0, 0]);
        }

        public Board(int size)
        {
            Grids = new string[size,size];
            RowSize = size;
        }

        public bool  AddWord(string word)
        {
            var wordArray = word.ToArray();
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
                    var insertWordResult = AttemptToAddWordVertically(wordArray, row, col);
                    matchFound = insertWordResult.Inserted;
                    if (matchFound) break;
                }
                row +=1;

            } while (row + wordArray.Length <= RowSize && matchFound == false);

            return matchFound;
        }

        private InsertWordResult AttemptToAddWordVertically(char[] wordCharArray, int currentRow, int col)
        {
            var wordLength = wordCharArray.Length;
            var word = wordCharArray.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

            //Add only if first cell has been prepopulated with first word; 
            if (string.IsNullOrEmpty(Grids[currentRow, col])) return new InsertWordResult();
            var wordInsertedResult = CanWordBeAddedVerticallyToRow(wordLength, currentRow, col, word);
            if (wordInsertedResult.Inserted)
            {
                AddWordVerticallyToRow(wordLength, currentRow, col, word);
            }
            return wordInsertedResult;
        }

        private InsertWordResult CanWordBeAddedVerticallyToRow(int wordLength, int row, int currentCol, string[] word)
        {
            var letterMatch = false;
            var letterMismatch = false;
            var currentRow = row;
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
                currentRow +=1;
            }

            var insertResult = new InsertWordResult
            {
                Inserted = letterMatch && !letterMismatch,
                StartCell = Tuple.Create(row, currentCol),
                EndCell = Tuple.Create(currentRow, currentCol),
                Word = word
            };

            var validateResult = ValidateSuffixRule(insertResult);
            if (validateResult)
            {
                insertResult.Inserted = true;
            }
            return insertResult;
        }

        private void AddWordVerticallyToRow(int wordLength, int row, int currentCol, string[] word)
        {
            var currentRow = row; 
            for (int i = currentRow; i < wordLength; i++)
            {
                if (Grids[currentRow, currentCol] != word[i])
                {
                    Grids[currentRow, currentCol] = word[i];
                }
                currentRow += 1;
            }
        }

        private void AddFirstWord(char[] wordArray)
        {
            for (int i = 0; i < wordArray.Length; i++)
            {
                Grids[0, i] = wordArray[i].ToString();
            }
        }

        private bool ValidateSuffixRule(InsertWordResult insertWordResult)
        {
            var nextrow = insertWordResult.EndCell.Item2;
            var col = insertWordResult.EndCell.Item1;
            return string.IsNullOrEmpty(Grids[nextrow, col]);
        }
    }
}
