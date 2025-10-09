using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utils {
    public static GameObject[] allVehiclesOfTags(params string[] tags) {
        List<GameObject[]> vehiclesByType = new List<GameObject[]>();
        foreach (string tag in tags) {
            vehiclesByType.Add(GameObject.FindGameObjectsWithTag(tag));
        }
        int totalAmt = 0;
        foreach (GameObject[] vehiclesOfTypeA in vehiclesByType) {
            totalAmt += vehiclesOfTypeA.Length;
        }
        GameObject[] vehicles = new GameObject[totalAmt];
        int index = 0;
        foreach (GameObject[] vehiclesOfTypeA in vehiclesByType) {
            foreach (GameObject vehicle in vehiclesOfTypeA) {
                vehicles[index] = vehicle;
                index++;
            }
        }
        return vehicles;
    }

    public static VehicleController aiControllerOfVehicle(GameObject vehicle) {
        VehicleController[] controllers = vehicle.GetComponents<VehicleController>();
        if (controllers.Length == 0) return null;
        if (controllers[0].GetType().IsAssignableFrom(controllers[1].GetType())) {
            return controllers[1];
        } else {
            return controllers[0];
        }
    }
}
