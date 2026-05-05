using System.Collections;
using System.IO;
using LawnDefense.Core;
using LawnDefense.Grid;
using LawnDefense.Placement;
using LawnDefense.Sun;
using LawnDefense.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LawnDefense.Tests.PlayMode
{
    public sealed class AutomatedGameplayAndUiSmokeTests
    {
        [UnityTest]
        public IEnumerator AugmentClickThenPlantCardClickCanPlacePlantAtWorldCoordinate()
        {
            yield return LoadMainAndChooseFirstAugment();

            PlantPlacementSystem placement = Object.FindObjectOfType<PlantPlacementSystem>();
            GridSystem grid = Object.FindObjectOfType<GridSystem>();
            SunSystem sun = Object.FindObjectOfType<SunSystem>();
            Assert.NotNull(placement, "Expected PlantPlacementSystem in Main scene.");
            Assert.NotNull(grid, "Expected GridSystem in Main scene.");
            Assert.NotNull(sun, "Expected SunSystem in Main scene.");

            PlantCardView firstCard = FindFirstActivePlantCard();
            Assert.NotNull(firstCard, "Expected at least one active plant card.");
            Button plantButton = firstCard.GetComponent<Button>();
            Assert.NotNull(plantButton, "Expected the first plant card to expose a Button.");

            int sunBefore = sun.Wallet.Current;
            plantButton.onClick.Invoke();
            yield return null;

            GridCoordinate coordinate = new GridCoordinate(2, 2);
            bool placed = placement.TryPlaceSelectedPlantAtWorld(grid.GridToWorld(coordinate));

            Assert.IsTrue(placed, "Expected selected plant to place at a valid empty world coordinate.");
            Assert.IsTrue(grid.TryGetCell(coordinate, out GridCell cell) && cell.IsOccupied, "Expected the target grid cell to be occupied after placement.");
            Assert.Less(sun.Wallet.Current, sunBefore, "Expected placement to spend sun.");
        }

        [UnityTest]
        public IEnumerator InitialAndPostAugmentUiCaptureHasValidLayout()
        {
            yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
            yield return WaitForFrames(3);

            Canvas.ForceUpdateCanvases();
            yield return CaptureScreenshot("augment-choice.bmp");
            AssertAugmentChoiceLayout();

            ClickFirstAugmentCard();
            yield return WaitForFrames(3);

            Canvas.ForceUpdateCanvases();
            yield return CaptureScreenshot("post-augment-hud.bmp");
            AssertHudAndPlantCardLayout();
        }

        private static IEnumerator LoadMainAndChooseFirstAugment()
        {
            yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
            yield return WaitForFrames(3);
            ClickFirstAugmentCard();
            yield return WaitUntilState(GameState.Playing, 2f);
        }

        private static IEnumerator WaitUntilState(GameState expected, float timeoutSeconds)
        {
            float end = Time.realtimeSinceStartup + timeoutSeconds;
            GameStateController controller = null;
            while (Time.realtimeSinceStartup < end)
            {
                controller = Object.FindObjectOfType<GameStateController>();
                if (controller != null && controller.CurrentState == expected)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("Expected game state " + expected + " but was " + (controller != null ? controller.CurrentState.ToString() : "missing controller") + ".");
        }

        private static IEnumerator WaitForFrames(int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
            }
        }

        private static void ClickFirstAugmentCard()
        {
            AugmentCardView[] cards = Object.FindObjectsOfType<AugmentCardView>(true);
            for (int i = 0; i < cards.Length; i++)
            {
                if (!cards[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                Button button = cards[i].GetComponent<Button>();
                Assert.NotNull(button, "Expected active augment card to expose a Button.");
                Assert.IsTrue(button.interactable, "Expected active augment card Button to be interactable.");
                button.onClick.Invoke();
                return;
            }

            Assert.Fail("Expected at least one active augment card.");
        }

        private static PlantCardView FindFirstActivePlantCard()
        {
            PlantCardView[] cards = Object.FindObjectsOfType<PlantCardView>(true);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].gameObject.activeInHierarchy)
                {
                    return cards[i];
                }
            }

            return null;
        }

        private static void AssertAugmentChoiceLayout()
        {
            AugmentCardView[] cards = Object.FindObjectsOfType<AugmentCardView>(true);
            int activeCount = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                if (!cards[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                activeCount++;
                AssertRectIsOnScreen(cards[i].GetComponent<RectTransform>(), cards[i].name);
                AssertVisibleTextsFit(cards[i].transform);
            }

            Assert.GreaterOrEqual(activeCount, 1, "Expected at least one visible augment card.");
        }

        private static void AssertHudAndPlantCardLayout()
        {
            GameHudView hud = Object.FindObjectOfType<GameHudView>();
            Assert.NotNull(hud, "Expected HUD view after augment selection.");
            AssertVisibleTextsFit(hud.transform);

            PlantCardView[] cards = Object.FindObjectsOfType<PlantCardView>(true);
            int activeCount = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                if (!cards[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                activeCount++;
                AssertRectIsOnScreen(cards[i].GetComponent<RectTransform>(), cards[i].name);
                AssertVisibleTextsFit(cards[i].transform);
            }

            Assert.GreaterOrEqual(activeCount, 3, "Expected at least three active plant cards.");
        }

        private static void AssertVisibleTextsFit(Transform root)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(false);
            for (int i = 0; i < texts.Length; i++)
            {
                Text text = texts[i];
                RectTransform rect = text.rectTransform;
                AssertRectIsOnScreen(rect, text.name);
                if (text.horizontalOverflow != HorizontalWrapMode.Wrap)
                {
                    Assert.LessOrEqual(text.preferredWidth, rect.rect.width + 4f, text.name + " text is wider than its RectTransform.");
                }

                Assert.LessOrEqual(text.preferredHeight, rect.rect.height + 4f, text.name + " text is taller than its RectTransform.");
            }
        }

        private static void AssertRectIsOnScreen(RectTransform rectTransform, string label)
        {
            Assert.NotNull(rectTransform, label + " is missing a RectTransform.");

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);

            for (int i = 0; i < corners.Length; i++)
            {
                Assert.IsTrue(screenRect.Contains(corners[i]), label + " corner " + i + " is outside the screen: " + corners[i]);
            }
        }

        private static IEnumerator CaptureScreenshot(string fileName)
        {
            if (Application.isBatchMode)
            {
                yield break;
            }

            yield return new WaitForEndOfFrame();

            string directory = Path.Combine(Application.dataPath, "../TestResults/Screenshots");
            Directory.CreateDirectory(directory);

            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();
            WriteBmp(Path.Combine(directory, fileName), screenshot.GetPixels32(), screenshot.width, screenshot.height);
            Object.Destroy(screenshot);
        }

        private static void WriteBmp(string path, Color32[] pixels, int width, int height)
        {
            int rowStride = width * 3;
            int padding = (4 - rowStride % 4) % 4;
            int pixelDataSize = (rowStride + padding) * height;

            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create, FileAccess.Write)))
            {
                writer.Write((byte)'B');
                writer.Write((byte)'M');
                writer.Write(54 + pixelDataSize);
                writer.Write(0);
                writer.Write(54);
                writer.Write(40);
                writer.Write(width);
                writer.Write(height);
                writer.Write((short)1);
                writer.Write((short)24);
                writer.Write(0);
                writer.Write(pixelDataSize);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);

                byte[] pad = new byte[padding];
                for (int y = 0; y < height; y++)
                {
                    int rowStart = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        Color32 color = pixels[rowStart + x];
                        writer.Write(color.b);
                        writer.Write(color.g);
                        writer.Write(color.r);
                    }

                    writer.Write(pad);
                }
            }
        }
    }
}
