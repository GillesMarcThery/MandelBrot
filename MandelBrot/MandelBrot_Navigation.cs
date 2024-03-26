using System.Windows;

namespace MandelBrot
{
    enum Status
    {
        None,
        Capture
    }
    struct Rectangle(Point TopLeft, Point BottomRight)
    {
        public Point TopLeft = TopLeft;
        public Point BottomRight = BottomRight;
        public double Width
        {
            get { return BottomRight.X - TopLeft.X; }
        }
        public double Height
        {
            get { return TopLeft.Y - BottomRight.Y; }
        }
    }
    internal class MandelBrot_Navigation
    {
        //Point InitialTopLeftCorner = new(-2, 1.2);
        //Size s = new Size(2.5, 2.4);
        //Point InitialBottomRightCorner = new(0.5, -1.2);
        //Point InitialTopLeftCorner = new(-2.5, 2.5);
        //Size s = new Size(5, 5);
        //Point InitialBottomRightCorner = new(2.5, -2.5);
        Point InitialTopLeftCorner = new(-2, 1.2);
        Size s = new Size(2.5, 2.4);
        Point InitialBottomRightCorner = new(0.5, -1.2);
        public Point temporaryUpperLeft;
        public Point temporaryBottomRight;
        List<Rectangle> myCollection = [];
        public int index = 0;
        public Status status = Status.None;
        public MandelBrot_Navigation()
        {
            myCollection.Add(new Rectangle(InitialTopLeftCorner, InitialBottomRightCorner));
        }
        public void Add_Rectangle(Point p1, Point p2)
        {
            if (p1 != p2)
                myCollection.Add(new Rectangle(p1, p2));
        }
        public void Supp_Rectangle(Point p1, Point p2)
        {
            myCollection.Remove(new Rectangle(p1, p2));
            if (index == myCollection.Count) index--;
        }

        public Point UpperLeft
        {
            get { return myCollection[index].TopLeft; }
        }
        public Point BottomRight
        {
            get { return myCollection[index].BottomRight; }
        }
        public bool Rewind()
        {
            if (index == 0) return false;
            index = 0;
            return true;
        }
        public bool End()
        {
            if (index == myCollection.Count - 1) return false;
            index = myCollection.Count - 1;
            return true;
        }
        public bool Previous()
        {
            if (index == 0) return false;
            index--;
            return true;
        }
        public bool Next()
        {
            if (index == myCollection.Count - 1) return false;
            index++;
            return true;
        }
        public double Width
        {
            get
            {
                return myCollection[index].Width;
            }
        }
        public double Height
        {
            get
            {
                return myCollection[index].Height;
            }
        }
    }
}
