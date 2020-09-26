using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossRoad : MonoBehaviour
{


    public List<CarAIController> carsForward = new List<CarAIController>();
    public List<CarAIController> carsBackward = new List<CarAIController>();
    public List<CarAIController> carsRight = new List<CarAIController>();
    public List<CarAIController> carsLeft = new List<CarAIController>();



    /// <summary>
    /// Return 'moves from' side
    /// </summary>
    /// <param name="toward"></param>
    /// <param name="side"></param>
    /// <returns>moves from 0 - foward, 1 - backward, 2 - left, 3 - right</returns>
    private int GetMovedFrom(float toward, float side)
    {
        int result = 0;
        if (toward > 0 && side < 0)
        {
            result = 0;
        }
        else if (toward < 0 && side > 0)
        {
            result = 1;
        }
        else if (toward < 0 && side < 0)
        {
            result = 2;
        }
        else
        {
            result = 3;
        }
        return result;
    }

    /// <summary>
    /// Add car to moves from list
    /// </summary>
    /// <param name="movesFrom"></param>
    /// <param name="car"></param>
    public void AddCarToList(int movesFrom, CarAIController car)
    {
        switch (movesFrom)
        {
            case 0:
                if (!carsForward.Contains(car))
                {
                    carsForward.Add(car);
                }
                break;
            case 1:
                if (!carsBackward.Contains(car))
                {
                    carsBackward.Add(car);
                }
                break;
            case 2:
                if (!carsLeft.Contains(car))
                {
                    carsLeft.Add(car);
                }
                break;
            case 3:
                if (!carsRight.Contains(car))
                {
                    carsRight.Add(car);
                }
                break;
        }

        SendCheckToAllCars();
    }

    /// <summary>
    /// Remove car from all moves lists
    /// </summary>
    /// <param name="car"></param>
    public void RemoveCarFromLists(CarAIController car)
    {
        carsForward.Remove(car);
        carsBackward.Remove(car);
        carsLeft.Remove(car);
        carsRight.Remove(car);

        car.CanGoToCrossroad = true;
        SendCheckToAllCars();
    }

    /// <summary>
    /// Search car in lists and return moves from direction
    /// </summary>
    /// <param name="car"></param>
    /// <returns></returns>
    private int ReturnCarsMovementDir(CarAIController car)
    {
        if (carsForward.Contains(car))
        {
            return 0;
        }
        else if (carsBackward.Contains(car))
        {
            return 1;
        }
        else if (carsLeft.Contains(car))
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    /// <summary>
    /// Check if have some car right
    /// </summary>
    /// <param name="curDir"></param>
    /// <returns></returns>
    private bool HasSomeoneRight(int curDir)
    {
        bool result = false;
        switch (curDir)
        {
            case 0:
                if (carsLeft.Count > 0) result = true;
                break;
            case 1:
                if (carsRight.Count > 0) result = true;
                break;
            case 2:
                if (carsBackward.Count > 0) result = true;
                break;
            case 3:
                if (carsForward.Count > 0) result = true;
                break;
        }

        //if (IsAllPathesBusy() && curDir == 2) result = false;

        return result;
    }

    /// <summary>
    /// Check if has some carf from front       Didn't work properly
    /// </summary>
    /// <param name="curDir"></param>
    /// <returns></returns>
    private bool CarTurningLeftOnCrossroad(int curDir)
    {
        bool result = false;
        switch (curDir)
        {
            case 0:
                if (carsBackward.Count > 0)
                {
                    foreach (CarAIController car in carsBackward)
                    {
                        if ((car.GetCarTurnPath() == 0 || car.GetCarTurnPath() == 1))
                        {
                            result = true;
                        }
                    }
                }
                break;
            case 1:
                if (carsForward.Count > 0)
                {
                    foreach (CarAIController car in carsForward)
                    {
                        if ((car.GetCarTurnPath() == 0 || car.GetCarTurnPath() == 1))
                        {
                            result = true;
                        }
                    }
                }
                break;
            case 2:
                if (carsRight.Count > 0)
                {
                    foreach (CarAIController car in carsRight)
                    {
                        if ((car.GetCarTurnPath() == 0 || car.GetCarTurnPath() == 1))
                        {
                            result = true;
                        }
                    }
                }
                break;
            case 3:
                if (carsLeft.Count > 0)
                {
                    foreach (CarAIController car in carsLeft)
                    {
                        if ((car.GetCarTurnPath() == 0 || car.GetCarTurnPath() == 1))
                        {
                            result = true;
                        }
                    }
                }
                break;
        }

        return result;
    }

    /// <summary>
    /// Check if car can go to the crossroad extended
    /// </summary>
    /// <param name="car"></param>
    /// <param name="someOneRight"></param>
    /// <param name="curDir"></param>
    /// <returns></returns>
    private bool CanGoToCrossroad(CarAIController car, bool someOneRight, int curDir)
    {
        if (!someOneRight /*&& (car.GetCarTurnPath() == 2 ? !CarTurningLeftOnCrossroad(curDir) : true)) */|| car.GetCarTurnPath() == 0)
        {
            return true;
        }
        else
        {
            if (IsAllPathesBusy() && curDir == 2) return true;
            else
                return false;
        }
    }

    private bool IsAllPathesBusy()
    {
        if (carsLeft.Count > 0 && carsRight.Count > 0 && carsBackward.Count > 0 && carsForward.Count > 0) return true;
        return false;
    }

    public void CanGoToCrossroad(CarAIController car)
    {
        bool canGo = CanGoToCrossroad(car, HasSomeoneRight(ReturnCarsMovementDir(car)), ReturnCarsMovementDir(car));
        car.CanGoToCrossroad = canGo;
    }

    private void SendCheckToAllCars()
    {
        foreach (CarAIController carAI in carsForward)
        {
            CanGoToCrossroad(carAI);
        }
        foreach (CarAIController carAI in carsBackward)
        {
            CanGoToCrossroad(carAI);
        }
        foreach (CarAIController carAI in carsLeft)
        {
            CanGoToCrossroad(carAI);
        }
        foreach (CarAIController carAI in carsRight)
        {
            CanGoToCrossroad(carAI);
        }
    }

    //  z   (-0.5, 0.3,  3.4)        3,364743       -0,4795978       forward
    // -z   ( 0.5, 0.3, -3.4)       -3,374835        0,4827945        backward
    // -x   (-3.4, 0.3, -0.5)      -0,4733841      -3,357418       left
    //  x   ( 3.4, 0.3,  0.5)         0,4828034       3,385496        right
}
