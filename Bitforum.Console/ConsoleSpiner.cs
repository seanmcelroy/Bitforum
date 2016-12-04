namespace Bitforum.Console
{
    public class ConsoleSpiner
    {
        int counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleSpiner"/> class.
        /// </summary>
        public ConsoleSpiner()
        {
            this.counter = 0;
        }

        public void Turn()
        {
            this.counter++;
            switch (this.counter % 4)
            {
                case 0:
                    System.Console.Write("/");
                    break;
                case 1:
                    System.Console.Write("-");
                    break;
                case 2:
                    System.Console.Write("\\");
                    break;
                case 3:
                    System.Console.Write("|");
                    break;
            }

            System.Console.SetCursorPosition(System.Console.CursorLeft - 1, System.Console.CursorTop);
        }
    }
}
