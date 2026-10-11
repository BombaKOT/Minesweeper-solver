//MINESWEEPER AUTOMATION PROJECT https://minesweeperonline.com/
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.DevTools.V143.Profiler;
using OpenQA.Selenium.Interactions;
MineSolver solver = new MineSolver();
solver.Init(256, 16, 16, 40, "https://minesweeperonline.com/#intermediate");
//solver.Init(480, 16, 30, 99, "https://minesweeperonline.com/");
public class MineSolver{

    int totalSquares = 81;
    public static int rows = 9;
    public static int columns = 9;
    public static int[,] field = new int[rows,columns];
    public static int[,] valueField = new int[rows,columns];
    public static int[,] surroudningField = new int[rows,columns];
    public static IWebDriver driver = new ChromeDriver();
    public static double[,] statisticField = new double[rows,columns];
    public int liveMines = 10;
    public int itterations = 1;
    string filePath = "FIELD.txt";
    public bool clickComplete;
    List<int[,]> clearedTiles = new List<int[,]>();
    List<int[,]> discoveredMines = new List<int[,]>();

    public void UpdateMap()
    {
        //Console.Write("\n MINES REMAINING: " + liveMines + " DISCOVERED MINES: " + discoveredMines.Count);
        itterations++;
        ////Console.Write("\n Loop: " + itterations);
        clearedTiles.Clear();
        clickComplete = false;

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.square")));
        if(liveMines > 0)
        {
            
            var elements = driver.FindElements(By.CssSelector("div.square"));
            List<string> classes = new List<string>();
            for(int i = 0; i < totalSquares; i++)
            {
                string value = elements[i].GetAttribute("class").ToString();
                classes.Add(value);
                
            }
            List<Mine> mines = new List<Mine>();
         
            for(int i = 0; i < totalSquares; i++)
            {
                Mine temp = new Mine();
                temp.amIClosed =  (classes[i] == "square blank");
                if (!temp.amIClosed)
                {
                    temp.intigerValue = int.Parse(Regex.Replace(classes[i], @"\D", ""));
                    Debug.Print("If this works i would not believe it");
                } else
                {
                    temp.intigerValue = 9;
                }
                mines.Add(temp);
            }
            for(int i = 0; i < rows; i++)
            { 
                for(int k = 0; k < columns; k++)
                {
                    
                    field[i, k] = mines[columns * i + k].intigerValue;
                }
            }
            CalculateOddsOfMine();
            int itr = 0;
            while(clearedTiles.Count == 0 && itr < 3)
            {
                for(int i = 0; i < rows; i++)
                {
                    for(int k = 0; k < columns; k++)
                    {
                        if(field[i,k] != 0 && field[i,k] != 9 && field[i,k] != 55)
                            DeduceTiles(i, k);
                    }
                }
                itr++;
            }
        } else
        {
            for(int i = 0; i < rows; i++)
                {
                    for(int k = 0; k < columns; k++)
                    {
                        if(field[i,k] == 9)
                            clearedTiles.Add(new int[i,k]);
                    }
                }
        }
        if(clearedTiles.Count == 0)
        {
            PickBestOdds();
        } else
        {
            ClearTiles();
        }
        ReadStatistics();
    }

    void DeduceTiles(int row, int column)
    {
        //column - 0 to columns
        //row - 0 to rows  
        int value = field[row, column];
        List<int[,]> surroundingTiles = GatherSurroundingTiles(row, column);  
        surroudningField[row, column] = surroundingTiles.Count; 
        for(int i = surroundingTiles.Count - 1; i >= 0; i--)
        {
            //Console.Write("Whats happening dimmensions : " + surroundingTiles[i].GetLength(0) + "x" + surroundingTiles[i].GetLength(1));
            if (statisticField[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] == 1 || field[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] == 55)
            {
                // getLength(0) - y coordinate
                // getength(1) - x axis
                field[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] = 55;
                value--;
                surroundingTiles.RemoveAt(i);
            } else if(clearedTiles.Contains(surroundingTiles[i])){
                surroundingTiles.RemoveAt(i);
            }
        } 

        valueField[row, column] = value;
        if(surroundingTiles.Count <= 0)
            return;

        if(value == 0)
        {
            for(int i =0; i < surroundingTiles.Count; i++)
            {
                clearedTiles.Add(surroundingTiles[i]);
            }
        } else
        {
            float val = value / surroundingTiles.Count;
            for(int i =0; i< surroundingTiles.Count; i++)
            {
                statisticField[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] = val;
            }
        }
    }

    public void Main()
    {
        File.WriteAllText(filePath, "");
        var blanksArray = driver.FindElements(By.CssSelector("div.square.blank"));
        blanksArray[columns * rows/2 + columns/2].Click();
        while (true)
        {
            try
            {
                UpdateMap();
                //Console.Write(itterations);
                itterations++;
            }
            catch (Exception exception)
            {
                try
                {
                    //<div class="facewin" style="margin-left:14px; margin-right: 14px;" id="face"></div>
                    driver.FindElement(By.CssSelector("div.facewin"));
                    WebDriverWait otherwait = new WebDriverWait(driver, TimeSpan.FromSeconds(4000));
                    IAlert alertI = otherwait.Until(ExpectedConditions.AlertIsPresent());
                    //Task.Delay(1000000);
                    var alert = driver.SwitchTo().Alert();
                    alert.SendKeys("Bomba");
                    //driver.SwitchTo().Alert();
                    //alert.SendKeys("Bomba");
                    // 3. Interact with the alert directly
                    //alert.SendKeys("Bomba");
                    //Thread.Sleep(600);
                    alert.Accept();
                    //alert.Accept();
                    var timer_seconds = driver.FindElement(By.Id("seconds_ones")).GetAttribute("class").ToString();
                    var timer_tens = driver.FindElement(By.Id("seconds_tens")).GetAttribute("class").ToString();
                    var timer_hundreds = driver.FindElement(By.Id("seconds_hundreds")).GetAttribute("class").ToString();
                    string timer = int.Parse(Regex.Replace(timer_hundreds, @"\D", "")).ToString() + int.Parse(Regex.Replace(timer_tens, @"\D", "")).ToString() + int.Parse(Regex.Replace(timer_seconds, @"\D", "")).ToString();
                    //Console.Write("TIME: " + timer);
                } catch (Exception otherException)
                {
                    //Console.Write("We lost its over i failed her im sorry - " + otherException);
                }
                //Console.Write("\n\nError - " + exception);
                break;
            }
        }
        /*try
            {
                var reset = driver.FindElement(By.CssSelector("div.facedead"));
                reset.Click();
            }
            catch
            {
                //Console.Write("\nWait we didnt loose???");
            }*/
        driver.Close();
        //ReadMap();
    }

    List<int[,]> GatherSurroundingTiles(int column, int row)
    {
        List<int[,]> surroundingTiles = new List<int[,]>();
        bool rightExists = (row + 1 < columns);
        bool leftExists = (row - 1 >= 0);
        bool bottomExists = (column+ 1 < rows);
        bool topExists = (column - 1 >= 0);
        
        if (topExists)
        {

            if(field[column - 1, row] == 9 || field[column - 1, row] == 55)
                surroundingTiles.Add(new int[column - 1, row]);

            if(rightExists)
                if(field[column - 1, row + 1] == 9 || field[column - 1, row + 1] == 55)
                    surroundingTiles.Add(new int[column - 1, row + 1]);
                

            if(leftExists)
                if(field[column - 1, row - 1] == 9 || field[column - 1, row - 1] == 55)
                    surroundingTiles.Add(new int[column - 1, row - 1]);
        }

        if (bottomExists)
        {

            if(field[column + 1, row] == 9 || field[column + 1, row] == 55)
                surroundingTiles.Add(new int[column + 1, row]);

            if(rightExists )
                if(field[column + 1, row + 1] == 9 || field[column + 1, row + 1] == 55)
                    surroundingTiles.Add(new int[column + 1, row + 1]);
                

            if(leftExists )
                if(field[column + 1, row - 1] == 9 || field[column + 1, row - 1] == 55)
                    surroundingTiles.Add(new int[column + 1, row - 1]);
                
        }
    
        if(rightExists)

            if(field[column, row + 1] == 9 || field[column, row + 1] == 55)
                surroundingTiles.Add(new int[column, row + 1]);
            

        if(leftExists)
            if(field[column, row - 1] == 9 || field[column, row - 1] == 55)
                surroundingTiles.Add(new int[column, row - 1]);
            

        return surroundingTiles;
        
    }

    void CalculateOddsOfMine()
    {
        int itr = 0;
        while(clearedTiles.Count == 0 && itr < 20)
        {
            for(int i =0; i < rows; i++)
            {
                for(int k = 0; k < columns; k++)
                {
                    int value = field[k, i];
                    if(value == 0 || value == 9 || value == 55)
                    {
                        return;
                    }
                    List<int[,]> surroundingTiles = GatherSurroundingTiles(k, i);
                    float avgWith = (float)field[k, i]/(float)surroundingTiles.Count;
                    for(int j = 0; j < surroundingTiles.Count; j++)
                    {
                        double currentStat = statisticField[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)];
                        if(currentStat == 1 || avgWith == 1)
                            field[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)] = 55;
                        if(currentStat == 11)
                        {
                            statisticField[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)] = avgWith;
                            return;
                        }
                       
                        double newStat = avgWith > currentStat ? avgWith : currentStat;
                        
                        statisticField[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)] = newStat;
                    }
                }
            }
            itr++;
        }
    }

    void PickBestOdds()
    {
        //Console.Write("\nPicking best odds");
        int[,] bestOddsCord = new int[0,0];
        double bestOdd = 10;
        for(int i = 0; i < rows; i++)
        {
            for(int k = 0; k < columns; k++)
            {
                if(statisticField[i, k] < bestOdd)
                {
                    bestOdd = statisticField[i, k];
                    bestOddsCord = new int[i,k];
                }  
            }
        }

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.square")));
        var elems = driver.FindElements(By.CssSelector("div.square"));
        string logOfChose = "\n" + "THE BEST ODD IS " + bestOdd + " AT " + bestOddsCord.GetLength(1) + "," + bestOddsCord.GetLength(0) + " ITTERATION: " + itterations + "\n";
        elems[bestOddsCord.GetLength(0) * columns + bestOddsCord.GetLength(1)].Click();
        File.AppendAllText(filePath, "\n Choice: " + bestOddsCord.GetLength(1) + "," + bestOddsCord.GetLength(0));
        clickComplete = true;
        File.AppendAllText(filePath, logOfChose);
    }
    void ReadMap()
    {

        //REMOVE CORE FUNCTION FROM DEBUG FUNCTION
        File.AppendAllText(filePath, "\n");
        List<string> foundmines = new List<string>();
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + field[i,k].ToString() + "]");
                if(field[i,k] == 55)
                {
                    bool containsMine = false;
                    foreach(int[,] certifiedMine in discoveredMines)
                    {
                        if (certifiedMine.GetLength(0) == i && certifiedMine.GetLength(1) == k)
                        {
                            containsMine = true;
                            foundmines.Add( "\n" + certifiedMine.GetLength(1).ToString() + " , "+certifiedMine.GetLength(0).ToString());
                            break;
                        }
                    }
                    if (!containsMine)
                    {
                        liveMines--;
                        discoveredMines.Add(new int[i,k]);
                    }
                }
            }
            row.Add("\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
        File.AppendAllText(filePath, string.Join("", foundmines));
    }
    void ReadStatistics()
    {
        File.WriteAllText(filePath, "\n");
        ReadMap();
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + statisticField[i,k].ToString() + "]");
            }
            File.AppendAllText(filePath, "\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
        ReadValues();
        ReadSurroundings();
        /*itterations++;
        if(itterations > totalSquares)
        {
            Environment.Exit(1);
        }*/
    }

    void ReadValues()
    {
        File.AppendAllText(filePath, "\n");
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + valueField[i,k].ToString() + "]");
            }
            File.AppendAllText(filePath, "\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
    }

    void ReadSurroundings()
    {
        File.AppendAllText(filePath, "\n");
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + surroudningField[i,k].ToString() + "]");
            }
            File.AppendAllText(filePath, "\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
    }

    void ClearTiles()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.square")));
        var elems = driver.FindElements(By.CssSelector("div.square"));
        for(int i = 0; i < clearedTiles.Count; i++)
        {
            elems[clearedTiles[i].GetLength(0) * columns + clearedTiles[i].GetLength(1)].Click();
            File.AppendAllText(filePath, "\n Choice: " + clearedTiles[i].GetLength(0) + "," + clearedTiles[i].GetLength(1));
            ReadMap();
        }
        clickComplete = true;
    }
    public void Init(int mapSize, int totalRows, int totalColumns, int liveMinesTotal, string url)
    {
        rows = totalRows;
        columns = totalColumns;
        totalSquares = mapSize;
        liveMines = liveMinesTotal;
        field = new int[rows,columns];
        valueField = new int[rows,columns];
        surroudningField = new int[rows,columns];
        statisticField = new double[rows,columns];
        driver.Navigate().GoToUrl(url);
        Main();
    }
}



class Mine
{
    public bool amIClosed;
    public int intigerValue;
}




// Mine solver 2

public class MineSolverUpgraded{

    int totalSquares = 81;
    public static int rows = 9;
    public static int columns = 9;
    public static int[,] field = new int[rows,columns];
    public static int[,] valueField = new int[rows,columns];
    public static int[,] surroudningField = new int[rows,columns];
    public static IWebDriver driver = new ChromeDriver();
    public static double[,] statisticField = new double[rows,columns];
    public int liveMines = 10;
    public int itterations = 1;
    string filePath = "FIELD.txt";
    public bool clickComplete;
    List<int[,]> clearedTiles = new List<int[,]>();
    List<int[,]> discoveredMines = new List<int[,]>();

    public void UpdateMap()
    {
        //Console.Write("\n MINES REMAINING: " + liveMines + " DISCOVERED MINES: " + discoveredMines.Count);
        itterations++;
        ////Console.Write("\n Loop: " + itterations);
        clearedTiles.Clear();
        clickComplete = false;

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.square")));
        if(liveMines > 0)
        {
            
            var elements = driver.FindElements(By.CssSelector("div.square"));
            List<string> classes = new List<string>();
            for(int i = 0; i < totalSquares; i++)
            {
                string value = elements[i].GetAttribute("class").ToString();
                classes.Add(value);
                
            }
            List<Mine> mines = new List<Mine>();
         
            for(int i = 0; i < totalSquares; i++)
            {
                Mine temp = new Mine();
                temp.amIClosed =  (classes[i] == "square blank");
                if (!temp.amIClosed)
                {
                    temp.intigerValue = int.Parse(Regex.Replace(classes[i], @"\D", ""));
                    Debug.Print("If this works i would not believe it");
                } else
                {
                    temp.intigerValue = 9;
                }
                mines.Add(temp);
            }
            for(int i = 0; i < rows; i++)
            { 
                for(int k = 0; k < columns; k++)
                {
                    
                    field[i, k] = mines[columns * i + k].intigerValue;
                }
            }
            CalculateOddsOfMine();
            int itr = 0;
            while(clearedTiles.Count == 0 && itr < 3)
            {
                for(int i = 0; i < rows; i++)
                {
                    for(int k = 0; k < columns; k++)
                    {
                        if(field[i,k] != 0 && field[i,k] != 9 && field[i,k] != 55)
                            DeduceTiles(i, k);
                    }
                }
                itr++;
            }
        } else
        {
            for(int i = 0; i < rows; i++)
                {
                    for(int k = 0; k < columns; k++)
                    {
                        if(field[i,k] == 9)
                            clearedTiles.Add(new int[i,k]);
                    }
                }
        }
        if(clearedTiles.Count == 0)
        {
            PickBestOdds();
        } else
        {
            ClearTiles();
        }
        ReadStatistics();
    }

    void DeduceTiles(int row, int column)
    {
        //column - 0 to columns
        //row - 0 to rows  
        int value = field[row, column];
        List<int[,]> surroundingTiles = GatherSurroundingTiles(row, column);  
        surroudningField[row, column] = surroundingTiles.Count; 
        for(int i = surroundingTiles.Count - 1; i >= 0; i--)
        {
            //Console.Write("Whats happening dimmensions : " + surroundingTiles[i].GetLength(0) + "x" + surroundingTiles[i].GetLength(1));
            if (statisticField[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] == 1 || field[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] == 55)
            {
                // getLength(0) - y coordinate
                // getength(1) - x axis
                field[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] = 55;
                value--;
                surroundingTiles.RemoveAt(i);
            } else if(clearedTiles.Contains(surroundingTiles[i])){
                surroundingTiles.RemoveAt(i);
            }
        } 

        valueField[row, column] = value;
        if(surroundingTiles.Count <= 0)
            return;

        if(value == 0)
        {
            for(int i =0; i < surroundingTiles.Count; i++)
            {
                clearedTiles.Add(surroundingTiles[i]);
            }
        } else
        {
            float val = value / surroundingTiles.Count;
            for(int i =0; i< surroundingTiles.Count; i++)
            {
                statisticField[surroundingTiles[i].GetLength(0), surroundingTiles[i].GetLength(1)] = val;
            }
        }
    }

    public void Main()
    {
        File.WriteAllText(filePath, "");
        var blanksArray = driver.FindElements(By.CssSelector("div.square.blank"));
        blanksArray[columns * rows/2 + columns/2].Click();
        while (true)
        {
            try
            {
                UpdateMap();
                //Console.Write(itterations);
                itterations++;
            }
            catch (Exception exception)
            {
                try
                {
                    //<div class="facewin" style="margin-left:14px; margin-right: 14px;" id="face"></div>
                    driver.FindElement(By.CssSelector("div.facewin"));
                    WebDriverWait otherwait = new WebDriverWait(driver, TimeSpan.FromSeconds(4000));
                    IAlert alertI = otherwait.Until(ExpectedConditions.AlertIsPresent());
                    //Task.Delay(1000000);
                    var alert = driver.SwitchTo().Alert();
                    alert.SendKeys("Bomba");
                    //driver.SwitchTo().Alert();
                    //alert.SendKeys("Bomba");
                    // 3. Interact with the alert directly
                    //alert.SendKeys("Bomba");
                    //Thread.Sleep(600);
                    alert.Accept();
                    //alert.Accept();
                    var timer_seconds = driver.FindElement(By.Id("seconds_ones")).GetAttribute("class").ToString();
                    var timer_tens = driver.FindElement(By.Id("seconds_tens")).GetAttribute("class").ToString();
                    var timer_hundreds = driver.FindElement(By.Id("seconds_hundreds")).GetAttribute("class").ToString();
                    string timer = int.Parse(Regex.Replace(timer_hundreds, @"\D", "")).ToString() + int.Parse(Regex.Replace(timer_tens, @"\D", "")).ToString() + int.Parse(Regex.Replace(timer_seconds, @"\D", "")).ToString();
                    //Console.Write("TIME: " + timer);
                } catch (Exception otherException)
                {
                    //Console.Write("We lost its over i failed her im sorry - " + otherException);
                }
                //Console.Write("\n\nError - " + exception);
                break;
            }
        }
        /*try
            {
                var reset = driver.FindElement(By.CssSelector("div.facedead"));
                reset.Click();
            }
            catch
            {
                //Console.Write("\nWait we didnt loose???");
            }*/
        driver.Close();
        //ReadMap();
    }

    List<int[,]> GatherSurroundingTiles(int column, int row)
    {
        List<int[,]> surroundingTiles = new List<int[,]>();
        bool rightExists = (row + 1 < columns);
        bool leftExists = (row - 1 >= 0);
        bool bottomExists = (column+ 1 < rows);
        bool topExists = (column - 1 >= 0);
        
        if (topExists)
        {

            if(field[column - 1, row] == 9 || field[column - 1, row] == 55)
                surroundingTiles.Add(new int[column - 1, row]);

            if(rightExists)
                if(field[column - 1, row + 1] == 9 || field[column - 1, row + 1] == 55)
                    surroundingTiles.Add(new int[column - 1, row + 1]);
                

            if(leftExists)
                if(field[column - 1, row - 1] == 9 || field[column - 1, row - 1] == 55)
                    surroundingTiles.Add(new int[column - 1, row - 1]);
        }

        if (bottomExists)
        {

            if(field[column + 1, row] == 9 || field[column + 1, row] == 55)
                surroundingTiles.Add(new int[column + 1, row]);

            if(rightExists )
                if(field[column + 1, row + 1] == 9 || field[column + 1, row + 1] == 55)
                    surroundingTiles.Add(new int[column + 1, row + 1]);
                

            if(leftExists )
                if(field[column + 1, row - 1] == 9 || field[column + 1, row - 1] == 55)
                    surroundingTiles.Add(new int[column + 1, row - 1]);
                
        }
    
        if(rightExists)

            if(field[column, row + 1] == 9 || field[column, row + 1] == 55)
                surroundingTiles.Add(new int[column, row + 1]);
            

        if(leftExists)
            if(field[column, row - 1] == 9 || field[column, row - 1] == 55)
                surroundingTiles.Add(new int[column, row - 1]);
            

        return surroundingTiles;
        
    }

    void CalculateOddsOfMine()
    {
        int itr = 0;
        while(clearedTiles.Count == 0 && itr < 20)
        {
            for(int i =0; i < rows; i++)
            {
                for(int k = 0; k < columns; k++)
                {
                    int value = field[k, i];
                    if(value == 0 || value == 9 || value == 55)
                    {
                        return;
                    }
                    List<int[,]> surroundingTiles = GatherSurroundingTiles(k, i);
                    float avgWith = (float)field[k, i]/(float)surroundingTiles.Count;
                    for(int j = 0; j < surroundingTiles.Count; j++)
                    {
                        double currentStat = statisticField[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)];
                        if(currentStat == 1 || avgWith == 1)
                            field[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)] = 55;
                        if(currentStat == 11)
                        {
                            statisticField[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)] = avgWith;
                            return;
                        }
                       
                        double newStat = avgWith > currentStat ? avgWith : currentStat;
                        
                        statisticField[surroundingTiles[j].GetLength(0), surroundingTiles[j].GetLength(1)] = newStat;
                    }
                }
            }
            itr++;
        }
    }

    void PickBestOdds()
    {
        //Console.Write("\nPicking best odds");
        int[,] bestOddsCord = new int[0,0];
        double bestOdd = 10;
        for(int i = 0; i < rows; i++)
        {
            for(int k = 0; k < columns; k++)
            {
                if(statisticField[i, k] < bestOdd)
                {
                    bestOdd = statisticField[i, k];
                    bestOddsCord = new int[i,k];
                }  
            }
        }

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.square")));
        var elems = driver.FindElements(By.CssSelector("div.square"));
        string logOfChose = "\n" + "THE BEST ODD IS " + bestOdd + " AT " + bestOddsCord.GetLength(1) + "," + bestOddsCord.GetLength(0) + " ITTERATION: " + itterations + "\n";
        elems[bestOddsCord.GetLength(0) * columns + bestOddsCord.GetLength(1)].Click();
        File.AppendAllText(filePath, "\n Choice: " + bestOddsCord.GetLength(1) + "," + bestOddsCord.GetLength(0));
        clickComplete = true;
        File.AppendAllText(filePath, logOfChose);
    }
    void ReadMap()
    {

        //REMOVE CORE FUNCTION FROM DEBUG FUNCTION
        File.AppendAllText(filePath, "\n");
        List<string> foundmines = new List<string>();
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + field[i,k].ToString() + "]");
                if(field[i,k] == 55)
                {
                    bool containsMine = false;
                    foreach(int[,] certifiedMine in discoveredMines)
                    {
                        if (certifiedMine.GetLength(0) == i && certifiedMine.GetLength(1) == k)
                        {
                            containsMine = true;
                            foundmines.Add( "\n" + certifiedMine.GetLength(1).ToString() + " , "+certifiedMine.GetLength(0).ToString());
                            break;
                        }
                    }
                    if (!containsMine)
                    {
                        liveMines--;
                        discoveredMines.Add(new int[i,k]);
                    }
                }
            }
            row.Add("\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
        File.AppendAllText(filePath, string.Join("", foundmines));
    }
    void ReadStatistics()
    {
        File.WriteAllText(filePath, "\n");
        ReadMap();
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + statisticField[i,k].ToString() + "]");
            }
            File.AppendAllText(filePath, "\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
        ReadValues();
        ReadSurroundings();
        /*itterations++;
        if(itterations > totalSquares)
        {
            Environment.Exit(1);
        }*/
    }

    void ReadValues()
    {
        File.AppendAllText(filePath, "\n");
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + valueField[i,k].ToString() + "]");
            }
            File.AppendAllText(filePath, "\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
    }

    void ReadSurroundings()
    {
        File.AppendAllText(filePath, "\n");
        for(int i = 0; i < rows; i++)
        {
            List<string> row = new List<string>();
            for(int k = 0; k < columns; k++)
            {
                row.Add("[" + surroudningField[i,k].ToString() + "]");
            }
            File.AppendAllText(filePath, "\n");
            ////Console.Write(string.Join("\n" + row.Count, row));
            File.AppendAllText(filePath, string.Join("", row));
        }
    }

    void ClearTiles()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.square")));
        var elems = driver.FindElements(By.CssSelector("div.square"));
        for(int i = 0; i < clearedTiles.Count; i++)
        {
            elems[clearedTiles[i].GetLength(0) * columns + clearedTiles[i].GetLength(1)].Click();
            File.AppendAllText(filePath, "\n Choice: " + clearedTiles[i].GetLength(0) + "," + clearedTiles[i].GetLength(1));
            ReadMap();
        }
        clickComplete = true;
    }
    public void Init(int mapSize, int totalRows, int totalColumns, int liveMinesTotal, string url)
    {
        rows = totalRows;
        columns = totalColumns;
        totalSquares = mapSize;
        liveMines = liveMinesTotal;
        field = new int[rows,columns];
        valueField = new int[rows,columns];
        surroudningField = new int[rows,columns];
        statisticField = new double[rows,columns];
        driver.Navigate().GoToUrl(url);
        Main();
    }
}



class Square
{
    public bool amIClosed;
    public int intigerValue;
    public bool containesMine;
    public List<int> surroundingTiles = new List<int>();
    public int leftOverValue;
}
