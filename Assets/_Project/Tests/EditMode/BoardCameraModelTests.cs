using NanaArrow.Gameplay.Input;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class BoardCameraModelTests
    {
        private const float Tolerance = 1e-4f;

        // 보드 반폭 (1.15, 1.35), 화면 반폭 (3.7, 8) — 줌 1 에서는 보드가 화면보다 작다
        private static BoardCameraModel NewModel(float zoomMax = 3f)
        {
            var model = new BoardCameraModel(1f, zoomMax, 0.3f);
            model.Configure(new Vector2(1.15f, 1.35f), new Vector2(3.7f, 8f), 0.4f);
            return model;
        }

        [Test]
        public void Configure_StartsAtZoomOne_Centered()
        {
            var model = NewModel();

            Assert.AreEqual(1f, model.Zoom, Tolerance);
            Assert.AreEqual(Vector2.zero, model.Offset);
            Assert.IsTrue(model.IsDefault);
        }

        [Test]
        public void Pan_AtZoomOne_IsIgnored()
        {
            var model = NewModel();

            model.Pan(new Vector2(1f, 1f));

            Assert.AreEqual(Vector2.zero, model.Offset);
        }

        [Test]
        public void SetZoom_ClampsToRange()
        {
            var model = NewModel(zoomMax: 3f);

            model.SetZoom(10f, Vector2.zero);
            Assert.AreEqual(3f, model.Zoom, Tolerance);

            model.SetZoom(0.2f, Vector2.zero);
            Assert.AreEqual(1f, model.Zoom, Tolerance);
            Assert.AreEqual(Vector2.zero, model.Offset);
        }

        [Test]
        public void SetZoom_KeepsFocusPointUnderFinger_WithinPanLimits()
        {
            // 줌 3 에서 가로 한계 = 1.15 + 0.4 - 3.7/3 = 0.3167. focus x=0.3 → 필요한 오프셋 0.3×(1-1/3)=0.2 는 한계 안
            var model = NewModel();
            var focus = new Vector2(0.3f, 0f);

            model.SetZoom(3f, focus);

            var before = focus.x / 3.7f;
            var after = (focus.x - model.Offset.x) / model.ViewHalfSize.x;
            Assert.AreEqual(0.2f, model.Offset.x, 1e-3f);
            Assert.AreEqual(before, after, 1e-3f);
        }

        [Test]
        public void SetZoom_WhenBoardStillFitsView_StaysCentered()
        {
            // 줌 2 에서는 보드+여백(1.55) < 화면 반폭(1.85) → 이동 불가, 초점과 무관하게 중앙
            var model = NewModel();

            model.SetZoom(2f, new Vector2(0.5f, 0.3f));

            Assert.AreEqual(Vector2.zero, model.Offset);
        }

        [Test]
        public void Pan_WhenZoomed_IsClampedToBoardPlusMargin()
        {
            var model = NewModel();
            model.SetZoom(3f, Vector2.zero);
            // 줌 3: 화면 반폭 (1.2333, 2.6667). 가로 한계 = 1.15 + 0.4 - 1.2333 = 0.3167, 세로는 보드+여백(1.75) < 화면(2.67) → 0
            model.Pan(new Vector2(10f, 10f));

            Assert.AreEqual(1.15f + 0.4f - 3.7f / 3f, model.Offset.x, 1e-3f);
            Assert.AreEqual(0f, model.Offset.y, Tolerance);

            model.Pan(new Vector2(-20f, 0f));
            Assert.AreEqual(-(1.15f + 0.4f - 3.7f / 3f), model.Offset.x, 1e-3f);
        }

        [Test]
        public void ZoomOut_ReclampsOffset_AndCentersAtZoomOne()
        {
            var model = NewModel();
            model.SetZoom(3f, Vector2.zero);
            model.Pan(new Vector2(10f, 0f));

            model.SetZoom(1f, Vector2.zero);

            Assert.AreEqual(Vector2.zero, model.Offset);
        }

        [Test]
        public void Reset_ReturnsToDefault()
        {
            var model = NewModel();
            model.SetZoom(2.5f, new Vector2(0.4f, 0.2f));
            model.Pan(new Vector2(0.1f, 0f));

            model.Reset();

            Assert.IsTrue(model.IsDefault);
        }

        [Test]
        public void RegisterEmptyTap_TwoTapsWithinWindow_ResetsAndReturnsTrue()
        {
            var model = NewModel();
            model.SetZoom(2f, Vector2.zero);

            Assert.IsFalse(model.RegisterEmptyTap(10f));
            Assert.IsTrue(model.RegisterEmptyTap(10.2f));
            Assert.IsTrue(model.IsDefault);
        }

        [Test]
        public void RegisterEmptyTap_TooSlow_IsNotDoubleTap()
        {
            var model = NewModel();
            model.SetZoom(2f, Vector2.zero);

            Assert.IsFalse(model.RegisterEmptyTap(10f));
            Assert.IsFalse(model.RegisterEmptyTap(10.5f));
            Assert.AreEqual(2f, model.Zoom, Tolerance);
        }

        [Test]
        public void RegisterEmptyTap_ThirdTapStartsNewSequence()
        {
            var model = NewModel();

            model.RegisterEmptyTap(1f);
            model.RegisterEmptyTap(1.1f);   // double → 리셋, 시퀀스 종료

            Assert.IsFalse(model.RegisterEmptyTap(1.2f), "리셋 직후의 탭은 새 시퀀스의 첫 탭");
        }
    }
}
