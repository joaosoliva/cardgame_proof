using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    public sealed class BoardController : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, BoardCellData> boardData = new Dictionary<Vector2Int, BoardCellData>();
        private readonly Dictionary<Vector2Int, BoardCellView> boardViews = new Dictionary<Vector2Int, BoardCellView>();

        private RectTransform boardRoot;
        private GridLayoutGroup grid;

        public bool ShowDebugLabels { get; set; }
        public Vector2Int BoardSize { get; private set; }

        public void BuildBoard(RectTransform parent, Vector2Int boardSize)
        {
            if (parent == null)
            {
                return;
            }

            BoardSize = boardSize;
            ClearBoard();

            GameObject rootObj = new GameObject("BoardRoot", typeof(RectTransform), typeof(Image));
            boardRoot = rootObj.GetComponent<RectTransform>();
            boardRoot.SetParent(parent, false);
            boardRoot.anchorMin = new Vector2(0.5f, 0.5f);
            boardRoot.anchorMax = new Vector2(0.5f, 0.5f);
            boardRoot.pivot = new Vector2(0.5f, 0.5f);

            float targetSize = Mathf.Min(parent.rect.width, parent.rect.height) - 64f;
            if (targetSize <= 0f)
            {
                targetSize = 760f;
            }

            boardRoot.sizeDelta = new Vector2(targetSize, targetSize);

            Image bg = rootObj.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.08f);

            grid = rootObj.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = boardSize.x;
            grid.spacing = new Vector2(8f, 8f);
            grid.padding = new RectOffset(8, 8, 8, 8);

            float cellSize = (targetSize - grid.padding.horizontal - ((boardSize.x - 1) * grid.spacing.x)) / boardSize.x;
            grid.cellSize = new Vector2(cellSize, cellSize);

            for (int y = 0; y < boardSize.y; y++)
            {
                for (int x = 0; x < boardSize.x; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    BoardCellData data = new BoardCellData
                    {
                        Coordinate = coord,
                        IsOccupied = false,
                        Occupant = null
                    };
                    boardData[coord] = data;

                    GameObject cellObj = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image), typeof(Button));
                    cellObj.transform.SetParent(boardRoot, false);
                    BoardCellView view = cellObj.AddComponent<BoardCellView>();
                    view.Initialize(coord, ShowDebugLabels, OnCellTapped);
                    boardViews[coord] = view;
                }
            }
        }

        public void ClearBoard()
        {
            boardData.Clear();
            boardViews.Clear();

            if (boardRoot != null)
            {
                Destroy(boardRoot.gameObject);
            }

            boardRoot = null;
            grid = null;
        }

        public bool ValidatePlacement(PlacedCardData cardData)
        {
            if (cardData == null)
            {
                return false;
            }

            if (!boardData.TryGetValue(cardData.Coordinate, out BoardCellData cell))
            {
                return false;
            }

            if (cell.IsOccupied)
            {
                return false;
            }

            return !string.IsNullOrEmpty(cardData.CardId);
        }

        public bool PlaceCard(PlacedCardData cardData)
        {
            if (!ValidatePlacement(cardData))
            {
                return false;
            }

            BoardCellData cell = boardData[cardData.Coordinate];
            cell.IsOccupied = true;
            cell.Occupant = cardData;
            boardData[cardData.Coordinate] = cell;

            BoardCellVisualState state = cardData.IsFaceUp ? BoardCellVisualState.Revealed : BoardCellVisualState.Hidden;
            if (CurrentPhaseIsSetup())
            {
                state = BoardCellVisualState.OccupiedSetup;
            }

            boardViews[cardData.Coordinate].SetState(state, CurrentPhaseIsSetup());
            return true;
        }

        public bool RemoveCard(Vector2Int coordinate)
        {
            if (!boardData.TryGetValue(coordinate, out BoardCellData cell) || !cell.IsOccupied)
            {
                return false;
            }

            cell.IsOccupied = false;
            cell.Occupant = null;
            boardData[coordinate] = cell;
            boardViews[coordinate].SetState(BoardCellVisualState.Empty, CurrentPhaseIsSetup());
            return true;
        }

        public bool RotateCard(Vector2Int coordinate)
        {
            if (!boardData.TryGetValue(coordinate, out BoardCellData cell) || !cell.IsOccupied || cell.Occupant == null)
            {
                return false;
            }

            cell.Occupant.IsFaceUp = !cell.Occupant.IsFaceUp;
            boardData[coordinate] = cell;
            boardViews[coordinate].SetState(cell.Occupant.IsFaceUp ? BoardCellVisualState.Revealed : BoardCellVisualState.Hidden, CurrentPhaseIsSetup());
            return true;
        }

        public void RefreshVisualsForPhase(GamePhase phase)
        {
            bool setup = phase == GamePhase.Setup || phase == GamePhase.TutorialIntro;
            foreach (KeyValuePair<Vector2Int, BoardCellData> pair in boardData)
            {
                BoardCellVisualState state = BoardCellVisualState.Empty;
                if (pair.Value.IsOccupied && pair.Value.Occupant != null)
                {
                    if (setup)
                    {
                        state = BoardCellVisualState.OccupiedSetup;
                    }
                    else
                    {
                        state = pair.Value.Occupant.IsFaceUp ? BoardCellVisualState.Revealed : BoardCellVisualState.Hidden;
                    }
                }

                boardViews[pair.Key].SetState(state, setup);
            }
        }

        private bool CurrentPhaseIsSetup()
        {
            GameController controller = FindFirstObjectByType<GameController>();
            return controller != null && (controller.CurrentPhase == GamePhase.Setup || controller.CurrentPhase == GamePhase.TutorialIntro);
        }

        private void OnCellTapped(Vector2Int coordinate)
        {
            if (ShowDebugLabels)
            {
                Debug.Log($"Cell tapped: {coordinate}");
            }
        }
    }
}
