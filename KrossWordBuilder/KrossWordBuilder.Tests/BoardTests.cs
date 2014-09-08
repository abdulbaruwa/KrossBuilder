﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KrossWordBuilder.Tests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void InitiatedBoard()
        {
            var board = new Board(12);
            Assert.IsNotNull(board.Grids);
            Assert.AreEqual(board.Grids.Length, 144);
        }

        [TestMethod]
        public void AddNewWordToBoard()
        {
            var board = new Board(12);
            board.AddWord("First");
            Assert.AreEqual(board.Grids[0, 0], "F");
            Assert.AreEqual(board.Grids[0, 1], "i");
            Assert.AreEqual(board.Grids[0, 2], "r");
            Assert.AreEqual(board.Grids[0, 3], "s");
            Assert.AreEqual(board.Grids[0, 4], "t");
        }
        
        [TestMethod]
        public void AddASecondWordVerticalIfFirstLetterMatches()
        {
            var board = new Board(12);
            board.AddWord("first");
            board.AddWord("restore");

            Assert.AreEqual("r", board.Grids[0, 2]);
            Assert.AreEqual("e", board.Grids[1, 2]);
            Assert.AreEqual("s", board.Grids[2, 2]);
            Assert.AreEqual("t", board.Grids[3, 2]);
            Assert.AreEqual("o", board.Grids[4, 2]);
            Assert.AreEqual("r", board.Grids[5, 2]);
            Assert.AreEqual("e", board.Grids[6, 2]);
        }

        [TestMethod]
        public void ShouldReturnAllSetCellsWhenRequested()
        {
            var board = new Board(12);
            board.AddWord("first");
            board.AddWord("restore");
            var cellsWithVals = board.GetLoadedCells();
            Assert.AreEqual(11, cellsWithVals.Count());
        }

        [TestMethod]
        public void ShouldReturnAllSetCellsWithStartCellsIdentified()
        {
            var board = new Board(12);
            board.AddWord("first");
            board.AddWord("restore");
            var cellsWithVals = board.GetLoadedCells();
            Assert.AreEqual(11, cellsWithVals.Count());
            Assert.AreEqual(2, cellsWithVals.Count(x => x.IsFirstLetter));
        }

        [TestMethod, Ignore]
        public void AddWordVerticalShouldFailIfThereIsCharInSuffixCell()
        {
            var board = new Board(12);
            board.Grids[7, 2] = "x";
            board.AddWord("first");
            var wordAdded = board.AddWord("restore");
            Assert.IsFalse(wordAdded);
        }

        [TestMethod]
        public void AddWordVerticalShouldFailIfThereIsCharInPrefixCell()
        {
            var board = new Board(12);
            board.Grids[7, 2] = "x";
            board.AddWord("first");
            var wordAdded = board.AddWord("restore");
            Assert.IsFalse(wordAdded);
        }

        //Test to determine cells with letters with match the current word to be inserted.
        //// If we know the direction of the existing word, say vertical, we can only then add new word horizonally
        [TestMethod]
        public void UnitTest_ShouldReturnCellsWithMatchesWithCurrentWordToBeInserted()
        {
            var board = new Board(12);
            board.AddWord("first");
            board.AddWord("restore");
            
            Assert.AreEqual(4, board.GetLetterMatchesFor("race").Count());
        }

        // Given a cell that is vertically occupied
        // That is Not in the first letter of a word
        // That is Not a junction letter
        // Test should assert a new word can only be inserted Horizonally

        [TestMethod]
        public void UnitTest_ShouldReturnHorizonalIfCellIsVerticallyUsedOnBoard()
        {
            var board = new Board(12);
            board.AddWord("first");
            board.AddWord("restore");

            Assert.IsTrue(board.IsCellVerticallyOccupied(new Cell(){Character = "r", Col = 2, Row = 6}));
        }
    }
}
