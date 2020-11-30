using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public struct Node
{
    public int x, y; // node position
    public int gCost; // walk cost
    public float hCost; // heuristic cost
    public bool[] directions;
    public int previousNode; // previous node index
}

public class Pathinding : MonoBehaviour
{
    // grid variables
    private int width = 30; // x length
    private int height = 30; // y length
    private float cellSize = 1.0f;
    private int[,] gridArray; // "level array"
    public bool[,] isOccupiedArray;
    public Vector2[] movesArray;
    public Node[] nodes;

    // cell references
    public Object normalCell; // normal enter cos
    public Object difficultCell; // double enter cost
    public Object waterCell; // double cost in both enter and exit
    public Object wallCell; // non walkable
    
    // starting and target positions
    public GameObject playerObject;
    public GameObject targetObject;
    private Vector2 startingPosition = new Vector2(1, 1);
    private Vector2 targetPosition = new Vector2(11, 11);
    public float inGameOffset = 5.0f;

    void Start()
    {
        // initialize arrays
        gridArray = new int[width, height];
        nodes = new Node[5000];

        isOccupiedArray = new bool[gridArray.GetLength(0), gridArray.GetLength(1)];
        for (int i = 0; i < isOccupiedArray.GetLength(0); i++)
        {
            for (int j = 0; j < isOccupiedArray.GetLength(1); j++)
                isOccupiedArray[i, j] = false;
        }

        GenerateTerrain();
        
    }

    private void Update()
    {
        // set ingame positions of starting object and target object
        playerObject.transform.position = startingPosition - new Vector2(inGameOffset, inGameOffset);
        targetObject.transform.position = targetPosition - new Vector2(inGameOffset, inGameOffset);
    }

    // randomly place fields on the map (starting and target positions can't be the wall)
    public void GenerateTerrain()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = Random.Range(0, 4);

                if (x == targetPosition.x && y == targetPosition.y)
                    gridArray[x, y] = Random.Range(0, 3);

                if (x == startingPosition.x && y == startingPosition.y)
                    gridArray[x, y] = Random.Range(0, 3);

                if (gridArray[x, y] == 0)
                    Instantiate(normalCell, new Vector2(transform.position.x + x * cellSize, transform.position.y + y * cellSize), transform.rotation);
                
                if (gridArray[x, y] == 1)
                    Instantiate(difficultCell, new Vector2(transform.position.x + x * cellSize, transform.position.y + y * cellSize), transform.rotation);
                
                if (gridArray[x, y] == 2)
                    Instantiate(waterCell, new Vector2(transform.position.x + x * cellSize, transform.position.y + y * cellSize), transform.rotation);
                
                if (gridArray[x, y] == 3)
                    Instantiate(wallCell, new Vector2(transform.position.x + x * cellSize, transform.position.y + y * cellSize), transform.rotation);
            }
        }
    }
    
    // check nearby fields (directions) and choose the one with the lowest cost to move to
    public int CheckNeighbours(int x, int y, Node currentNode)
    {
        bool[] canMoveArray = new bool[8];
        float[] fCostArray = new float[8];

        // fill array with false
        for (int i = 0; i < canMoveArray.Length; i++)
            canMoveArray[i] = false;

        //Debug.Log("current x = " + x);
        //Debug.Log("current y = " + y);

        // upper
        if (y + 1 <= gridArray.GetLength(1) - 1)
        {
            if (gridArray[x, y + 1] != 3 && currentNode.directions[0] == false && isOccupiedArray[x, y + 1] == false)
            {
                canMoveArray[0] = true;
                fCostArray[0] = CalculateFCost(currentNode, x, y + 1, false);
            }
        }
        
        // right
        if (x + 1 <= gridArray.GetLength(0) - 1)
        {
            if (gridArray[x + 1, y] != 3 && currentNode.directions[2] == false && isOccupiedArray[x + 1, y] == false)
            {
                canMoveArray[2] = true;
                fCostArray[2] = CalculateFCost(currentNode, x + 1, y, false);
            }
        }

        // bottom
        if (y - 1 >= 0)
        {
            if (gridArray[x, y - 1] != 3  && currentNode.directions[4] == false && isOccupiedArray[x, y - 1] == false)
            {
                canMoveArray[4] = true;
                fCostArray[4] = CalculateFCost(currentNode, x, y - 1, false);
            }
        }
        
        // left
        if (x - 1 >= 0)
        {
            if (gridArray[x - 1, y] != 3 && currentNode.directions[6] == false && isOccupiedArray[x - 1, y] == false)
            {
                canMoveArray[6] = true;
                fCostArray[6] = CalculateFCost(currentNode, x - 1, y, false);
            }
        }
        
        // upper right
        if (y + 1 <= gridArray.GetLength(1) - 1 && x + 1 <= gridArray.GetLength(0) - 1)
        {
            if (gridArray[x, y + 1] != 3 && gridArray[x + 1, y + 1] != 3 && gridArray[x + 1, y] != 3  && currentNode.directions[1] == false  && isOccupiedArray[x + 1, y + 1] == false)
            {
                canMoveArray[1] = true;
                fCostArray[1] = CalculateFCost(currentNode, x + 1, y + 1, true);
            }
        }

        // bottom right
        if (y - 1 >= 0 && x + 1 <= gridArray.GetLength(0) - 1)
        {
            if (gridArray[x, y - 1] != 3 && gridArray[x + 1, y - 1] != 3 && gridArray[x + 1, y] != 3 && currentNode.directions[3] == false  && isOccupiedArray[x + 1, y - 1] == false)
            {
                canMoveArray[3] = true;
                fCostArray[3] = CalculateFCost(currentNode, x + 1, y - 1, true);
            }
        }

        // bottom left
        if (y - 1 >= 0 && x - 1 >= 0)
        {
            if (gridArray[x, y - 1] != 3 && gridArray[x - 1, y - 1] != 3 && gridArray[x - 1, y] != 3 && currentNode.directions[5] == false  && isOccupiedArray[x - 1, y - 1] == false)
            {
                canMoveArray[5] = true;
                fCostArray[5] = CalculateFCost(currentNode, x - 1, y - 1, true);
            }
        }

        // upper left
        if (y + 1 <= gridArray.GetLength(0) - 1 && x - 1 >= 0)
        {
            if (gridArray[x, y + 1] != 3 && gridArray[x - 1, y + 1] != 3 && gridArray[x - 1, y] != 3  && currentNode.directions[7] == false  && isOccupiedArray[x - 1, y + 1] == false)
            {
                canMoveArray[7] = true;
                fCostArray[7] = CalculateFCost(currentNode, x - 1, y + 1, true);
            }
        }

        int index = -1;
        float smallestEnterCost = 10000; // we are going to modify this, so the initial value have to be high
        
        // find the direction with the lowest cost
        for (int i = 0; i < canMoveArray.Length; i++)
        {
            if (canMoveArray[i] == true)
            {    
                //Debug.Log("can move index = " + i + " f cost = " + fCostArray[i]);
                if (fCostArray[i] < smallestEnterCost)
                {
                    smallestEnterCost = fCostArray[i];
                    index = i;
                }
            }
            else
            {
                //Debug.Log("can't move index = " + i);
            }
        }
        
        //Debug.Log("smallest cost = " + smallestEnterCost);
        return index;
    }

    // calculate full cost
    public float CalculateFCost(Node currentNode, int x, int y, bool isDiagonal)
    {
        // calculate g cost based on field type and if its diagonal or strait movement
        float gCost = currentNode.gCost;
        
        // normal field
        if (gridArray[currentNode.x, currentNode.y] == 0)
        {
            if (isDiagonal)
                gCost += 14;
            else
                gCost += 10;
        }
        
        // difficult field
        if (gridArray[currentNode.x, currentNode.y] == 1)
        {
            if (isDiagonal)
                gCost += 14 * 2;
            else
                gCost += 10 * 2;
        }
        
        // water field
        if (gridArray[currentNode.x, currentNode.y] == 2)
        {
            if (isDiagonal)
                gCost += 14 * 4;
            else
                gCost += 10 * 4;
        }
        
        // calculate f cost from the formula : (f(x) = g(x) + h(x))
        float hCost = CalculateHCost(x, y, targetPosition);
        float fCost = gCost + hCost;

        return fCost;
    }
    
    // calculate heuristic cost
    public float CalculateHCost(int x, int y, Vector2 targetPosition)
    {
        Vector2 current = new Vector2(x, y);
        float distance = Vector2.Distance(current, targetPosition) * 10;

        return distance;
    }

    
    public void FindPath(Vector2 startingPosition, Vector2 targetPosition)
    {
        int endNode = -1;
        int nodeCounter = 0;
        int maxIterations = 5000;
        
        // create first node
        nodes[nodeCounter] = new Node();
        nodes[nodeCounter].x = (int) startingPosition.x;
        nodes[nodeCounter].y = (int) startingPosition.y;
        nodes[nodeCounter].directions = new bool[8];
        nodes[nodeCounter].previousNode = -1;
        
        for (int j = 0; j < 8; j++)
            nodes[nodeCounter].directions[j] = false;
        
        nodes[nodeCounter].gCost = 0;
        nodes[nodeCounter].hCost = CalculateHCost(nodes[nodeCounter].x, nodes[nodeCounter].y, targetPosition);
        
        nodeCounter++;

        while (endNode == -1 && nodeCounter < nodes.Length && maxIterations > 0)
        {
            int forint = nodeCounter;

            for (int i = 0; i < forint; i++)
            {
                maxIterations--;
                
                //declare and initialize directions we want move to
                int direction = CheckNeighbours(nodes[i].x, nodes[i].y, nodes[i]);

                int x = 0;
                int y = 0;
                bool canMove = true;
                
                switch (direction)
                {
                    case 0:
                        x = 0;
                        y = 1;
                        break;
                    case 1:
                        x = 1;
                        y = 1;
                        break;
                    case 2:
                        x = 1;
                        y = 0;
                        break;
                    case 3:
                        x = 1;
                        y = -1;
                        break;
                    case 4:
                        x = 0;
                        y = -1;
                        break;
                    case 5:
                        x = -1;
                        y = -1;
                        break;
                    case 6:
                        x = -1;
                        y = 0;
                        break;
                    case 7:
                        x = -1;
                        y = 1;
                        break;
                    case -1:
                        canMove = false;
                        break;
                }
                
                // create next nodes to search for the target point
                if (canMove && isOccupiedArray[nodes[i].x + x, nodes[i].y + y] == false && nodeCounter < nodes.Length)
                {
                    Debug.DrawLine(new Vector3(nodes[i].x - inGameOffset, nodes[i].y - inGameOffset, 0), new Vector3(nodes[i].x - inGameOffset + x, nodes[i].y - inGameOffset + y, 0), Color.red, 100.0f); // shows all steps as a red line
                    
                    nodes[nodeCounter] = new Node();
                    nodes[nodeCounter].directions = new bool[8];

                    if (direction != -1)
                    {
                        for (int j = 0; j < 8; j++)
                            nodes[nodeCounter].directions[j] = false;
                    }

                    nodes[nodeCounter].directions[FlipDirection(direction)] = true;

                    nodes[i].directions[direction] = true;

                    nodes[nodeCounter].x = nodes[i].x + x;
                    nodes[nodeCounter].y = nodes[i].y + y;
                    
                    isOccupiedArray[nodes[nodeCounter].x, nodes[nodeCounter].y] = true;
                    
                    // calculate g cost based on field type and if its diagonal or straight movement so we can add suitable cost to the next node
                    int gCost = 0;
                    bool isDiagonal = false;
                    
                    // diagonal movement directions
                    if (direction == 1 || direction == 3 || direction == 5 || direction == 7)
                        isDiagonal = true;
                    
                    // normal field
                    if (gridArray[nodes[nodeCounter].x, nodes[nodeCounter].y] == 0)
                    {
                        if (isDiagonal)
                            gCost += 14;
                        else
                            gCost += 10;
                    }
                    
                    // difficult field
                    if (gridArray[nodes[nodeCounter].x, nodes[nodeCounter].y] == 1)
                    {
                        if (isDiagonal)
                            gCost += 14 * 2;
                        else
                            gCost += 10 * 2;
                    }
            
                    // water field
                    if (gridArray[nodes[nodeCounter].x, nodes[nodeCounter].y] == 2)
                    {
                        if (isDiagonal)
                            gCost += 14 * 4;
                        else
                            gCost += 10 * 4;
                    }
                    
                    nodes[nodeCounter].gCost = nodes[i].gCost + gCost;
                    nodes[nodeCounter].hCost = CalculateHCost(nodes[nodeCounter].x, nodes[nodeCounter].y, targetPosition);
                    nodes[nodeCounter].previousNode = i;
                    
                    if (nodes[nodeCounter].x == targetPosition.x && nodes[nodeCounter].y == targetPosition.y)
                    {
                        endNode = nodeCounter;
                        goto found;
                    }
                    
                    nodeCounter++;
                }
            }
        }
        
        found:
        Debug.Log("steps :" + (nodeCounter - 1));
        
        // if reached end node
        if (endNode != -1)
        {
           Debug.Log("found path to the target point successfully"); 
           GetPath(nodes[endNode]);
        }
        else
            Debug.Log("cannot found the path to the target point");
    }

    // shows shortest path and total costs
    public void GetPath(Node endNode)
    {
        int index = endNode.previousNode;
        Node currentNode = endNode;
        Node previousNode = nodes[index];
        
        int iterator = 0;
        
        movesArray = new Vector2[1000];
        
        float totalGCost = 0;
        float totalFCost = 0;
        
        // iterates from the last point to the starting point
        while (currentNode.previousNode != -1)
        {
            totalGCost += currentNode.gCost;
            totalFCost += currentNode.hCost + currentNode.gCost;
            
            Debug.DrawLine(new Vector3(currentNode.x - inGameOffset, currentNode.y - inGameOffset, 0), new Vector3(previousNode.x - inGameOffset, previousNode.y - inGameOffset, 0), Color.yellow, 100.0f); // shows shortest path as a yellow line
            
            movesArray[iterator] = new Vector2(currentNode.x, currentNode.y) - new Vector2(previousNode.x, previousNode.y);
            currentNode = previousNode;

            if (currentNode.previousNode != -1)
                previousNode = nodes[currentNode.previousNode];

            iterator++;
        }
        
        Debug.Log("total g Cost = " + totalGCost + " , total f Cost = " + (int) totalFCost);
    }

    // flip direction so we can't move backwards
    public int FlipDirection(int dir)
    {
        int ret = -1;

        if (dir == 0)
            ret = 4;

        if (dir == 2)
            ret = 6;

        if (dir == 4)
            ret = 0;

        if (dir == 6)
            ret = 2;

        if (dir == 1)
            ret = 5;

        if (dir == 3)
            ret = 7;

        if (dir == 5)
            ret = 3;

        if (dir == 7)
            ret = 3;

        return ret;
    }

    public void OnButtonClick()
    {
        FindPath(startingPosition, targetPosition);
    }


}


