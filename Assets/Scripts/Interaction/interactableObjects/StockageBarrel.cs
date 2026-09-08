using UnityEngine;
using System;

public class StockageBarrel : MonoBehaviour
{
    private const int maxCount = 9;
    private int currentCountGrounded;
    private int currentCountNotGrounded;

    // percent
    private int fillPercentGrounded;
    private int fillPercentNotGrounded;
    private int maxPercentage = 100;

    public int FillPercent => (CurrentTotalCount * maxPercentage) / maxCount;
    public int CurrentTotalCount => currentCountGrounded + currentCountNotGrounded;
    public bool IsFullyGround => currentCountNotGrounded == 0 && currentCountGrounded > 0;

    public event Action<int> OnGroundedStockSent;
    public event Action OnBarrelStateChanged;

    private void Start()
    {
        ResetStockageBarrel();
    }
    public bool TryAddProductIntoBarrel()
    {
        if(CurrentTotalCount < maxCount)
        {
            currentCountNotGrounded ++;
            fillPercentNotGrounded = PercentTransformer(currentCountNotGrounded);
            fillPercentGrounded = PercentTransformer(currentCountGrounded);
            OnBarrelStateChanged?.Invoke();
            return true;
        }
        else
        {
               return false;     
        }
    }

    public void TryPumpToCuve()
    {
        if (!IsFullyGround)
        {
            return; // feedback needed
        }
        else
        {
            int quantitySent = currentCountGrounded;
            OnGroundedStockSent?.Invoke(quantitySent);
            ResetStockageBarrel();
        }
    }

    public void Ground()
    {
        if (currentCountNotGrounded >= 1)
        {
            currentCountNotGrounded --;
            currentCountGrounded++;
            fillPercentNotGrounded = PercentTransformer(currentCountNotGrounded);
            fillPercentGrounded = PercentTransformer(currentCountGrounded);
            OnBarrelStateChanged?.Invoke();
        }
        else
        {
             return; // mettre un feedback d inutilite 
        }
    }

    public int PercentTransformer(int pCurrentCountUnit)
    {
       return (pCurrentCountUnit * maxPercentage) / maxCount;
    }

    private void ResetStockageBarrel()
    {
        currentCountGrounded = 0;
        currentCountNotGrounded = 0;
        fillPercentGrounded = 0;
        fillPercentNotGrounded = 0;
    }
}
