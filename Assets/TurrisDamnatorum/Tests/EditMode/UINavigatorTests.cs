using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    public class UINavigatorTests
    {
        //   [0]        [3]
        //   [1]        [4]
        //   [2]
        //        [5 – szeroki przycisk na dole]
        static readonly Rect[] Layout =
        {
            new Rect(0, 0, 100, 40), new Rect(0, 50, 100, 40), new Rect(0, 100, 100, 40),
            new Rect(300, 0, 100, 40), new Rect(300, 50, 100, 40),
            new Rect(100, 300, 300, 50),
        };

        static readonly Vector2 Up = new Vector2(0, -1), Down = new Vector2(0, 1), Left = new Vector2(-1, 0), Right = new Vector2(1, 0);

        [Test]
        public void MovesWithinColumn()
        {
            Assert.AreEqual(1, UINavigator.FindInDirection(Layout, 0, Down));
            Assert.AreEqual(2, UINavigator.FindInDirection(Layout, 1, Down));
            Assert.AreEqual(0, UINavigator.FindInDirection(Layout, 1, Up));
            Assert.AreEqual(-1, UINavigator.FindInDirection(Layout, 0, Up), "Brak celu – fokus zostaje");
        }

        [Test]
        public void MovesBetweenColumns_PreferringSameRow()
        {
            Assert.AreEqual(4, UINavigator.FindInDirection(Layout, 1, Right));
            Assert.AreEqual(0, UINavigator.FindInDirection(Layout, 3, Left));
            Assert.AreEqual(-1, UINavigator.FindInDirection(Layout, 3, Right));
        }

        [Test]
        public void ReachesBottomButton()
        {
            Assert.AreEqual(5, UINavigator.FindInDirection(Layout, 2, Down));
            Assert.AreEqual(5, UINavigator.FindInDirection(Layout, 4, Down));
        }
    }
}
