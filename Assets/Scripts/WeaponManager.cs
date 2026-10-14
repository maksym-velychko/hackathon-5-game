using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public GameObject weapon1;
    public GameObject weapon2;

    void Start()
    {
        SwitchWeapon(1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            SwitchWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
{
            SwitchWeapon(2);
        }
    }

    private void SwitchWeapon(int weaponIndex)
    {
        if (weaponIndex == 1){
            if (weapon1 != null) weapon1.SetActive(true);
            if (weapon2 != null) weapon2.SetActive(false);
        }
        else if (weaponIndex == 2){
            if (weapon1 != null) weapon1.SetActive(false);
            if (weapon2 != null) weapon2.SetActive(true);
        }
    }
}