using System.Collections.Generic;
using System.Linq;
using Collapse.Blocks;
using UnityEngine;

namespace Collapse {
    /**
     * Partial class for separating the main functions that are needed to be modified in the context of this test
     */
    public partial class BoardManager {

        public void ClearBombFromList(Bomb bomb)
        {
            if (m_activeBombs.Contains(bomb))
                m_activeBombs.Remove(bomb);

            if (m_activeBombs.Count > 0)
            {
                if(!m_activeBombs.All(b => b == null))
                {
                    m_activeBombs.Clear();
                }
                else
                {
                    return;
                }
            }

            // Regenerate
            ScheduleRegenerateBoard();
        }

        /**
         * Trigger a bomb
         */
        public void TriggerBomb(Bomb bomb) {

            //Get all the blocks surrounding the bomb.
            var results = new List<Block>();
            FindBombBlocks(bomb.GridPosition.x, bomb.GridPosition.y, results);

            // Trigger blocks
            for (var i = 0; i < results.Count; i++)
            {
                if (results[i].Type == BlockType.Bomb)
                {
                    Bomb testBomb = (Bomb)results[i];

                    if (testBomb == null)
                        return;

                    //Don't regenerate the board if you triggered another bomb.
                    if (testBomb != bomb)
                    {
                        //If the bomb is already active don't trigger it again.
                        if (!m_activeBombs.Contains(testBomb))
                        {
                            m_activeBombs.Add(testBomb);
                            testBomb.Triger(m_bombSequenceTriggerDelaySpeed);
                        }
                    }
                }
                else
                {
                    results[i].Triger(0.0f);
                }
            }
        }

        /**
         * Trigger a match
         */
        public void TriggerMatch(Block block) {
            // Find all blocks in this match
            var results = new List<Block>();
            var tested = new List<(int row, int col)>();
            FindChainRecursive(block.Type, block.GridPosition.x, block.GridPosition.y, tested, results);

            //Keep the time needed to delay before regenerating the board.
            float timeToWaitBeforeRegenerating = (results.Count - 1) * m_blocksSequenceTriggerDelaySpeed;
            // Trigger blocks
            for (var i = 0; i < results.Count; i++)
            {
                results[i].Triger(i * m_blocksSequenceTriggerDelaySpeed);
            }

            // Regenerate
            Invoke("ScheduleRegenerateBoard", timeToWaitBeforeRegenerating);
        }


        private void FindBombBlocks(int col, int row, List<Block> results)
        {
            Block testBlock = null;

            //Go over all the blocks surrounding the bomb.
            for (int testCol = col - 1; testCol <= col + 1; testCol++)
            {
                //If the grid array is out of bounds continue.
                if (testCol >= blocks.GetLength(0) || testCol < 0)
                    continue;

                for (int testRow = row - 1; testRow <= row + 1; testRow++)
                {
                    if (testRow >= blocks.GetLength(1) || testRow < 0)
                        continue;

                    testBlock = blocks[testCol, testRow];

                    //If the block is valid add it to the results.
                    if (testBlock == null)
                        continue;

                    if (results.Contains(testBlock))
                        continue;

                    results.Add(testBlock);
                }
            }

        }
    
        /**
         * Recursively collect all neighbors of same type to build a full list of blocks in this "chain" in the results list
         */
        private void FindChainRecursive(BlockType type, int col, int row, List<(int row, int col)> testedPositions,
            List<Block> results) {

            //Add the initial block if not already added.
            if(!results.Contains(blocks[col,row]))
                results.Add(blocks[col, row]);

            //Create a list to hold all the matched blocks.
            List<Block> matchedBlocks = new List<Block>();

            //If the block is matching the type of block tested add it to the results if not added.
            if (IsBlockMatching(type,col,row + 1,testedPositions))
            {
                Block testBlock = blocks[col, row + 1];
                if (!results.Contains(testBlock))
                {
                    results.Add(testBlock);
                    matchedBlocks.Add(testBlock);
                }
            }

            if (IsBlockMatching(type, col, row - 1, testedPositions))
            {
                Block testBlock = blocks[col, row - 1];
                if (!results.Contains(testBlock))
                {
                    results.Add(testBlock);
                    matchedBlocks.Add(testBlock);
                }
            }
            
            if (IsBlockMatching(type, col + 1, row, testedPositions))
            {
                Block testBlock = blocks[col + 1, row];
                if (!results.Contains(testBlock))
                {
                    results.Add(testBlock);
                    matchedBlocks.Add(testBlock);
                }
            }

            if (IsBlockMatching(type, col - 1, row, testedPositions))
            {
                Block testBlock = blocks[col - 1, row];
                if (!results.Contains(testBlock))
                {
                    results.Add(testBlock);
                    matchedBlocks.Add(testBlock);
                }
            }

            //If there was no match don't continue.
            if (matchedBlocks.Count == 0)
                return;

            //Call the function on every matched block found.
            foreach (Block block in matchedBlocks)
            {
                FindChainRecursive(block.Type, block.GridPosition.x, block.GridPosition.y, testedPositions, results);
            }

        }

        /*
         * Check if the block in the given col and row match the type.
         */
        private bool IsBlockMatching(BlockType type,int col,int row, List<(int row, int col)> testedPositions)
        {
            //If you are out of array bounds the block doesn't match.
            if (col >= blocks.GetLength(0) || col < 0)
                return false;

            if (row >= blocks.GetLength(1) || row < 0)
                return false;

            //If you already tested the position no need to test again.
            if (testedPositions.Contains((col, row)))
                return false;

            testedPositions.Add((col, row));
            Block testBlock = blocks[col, row];

            if (testBlock == null || testBlock.Type != type)
                return false;

            return true;
        }

    }
}