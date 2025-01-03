# *.itemdisplayrules structure

The itemdisplayrules file is used to store the `ItemDisplayRules` for each item in the mod. At the top of the hierarchy is the `DisplayRules` group. This group is the top level and exists for validation. 
```json
{
    "DisplayRules" : {...}
}
```
Below that is a group for every item. These groups are keyed to the class names of each item, not their in game name.
```json
{
    "DisplayRules" : {
        "Mouthwash" : {...},
        "StasisRifle" : {...},
        "Ouroboros" : {...},
        "AggroDown" : {...},
        "AggroUp" : {...},
        ...
    }
}
```
Within those item groups are more groups, which contain the actual `ItemDisplayRule` information. They are keyed to the body name to avoid naming conflicts.
```json
{
    "DisplayRules" : {
        "Mouthwash" : {
            "CaptainBody": {
                "modelName" : "mdlCaptain",
                "childName": "ThighL",
                "localPos": "0.09907F,0.31531F,0.12536F",
                "localAngles": "5.91019F,233.3841F,182.2552F",
                "localScale": "0.68848F,0.68848F,0.68848F"
            },
            "CrocoBody": {...},
            ...
        },
        ...
    }
}
```
The ItemDisplayRule groups are generated by first downloading and installing [ItemDisplayPlacementHelper](https://thunderstore.io/c/riskofrain2/p/KingEnderBrine/ItemDisplayPlacementHelper/) by KingEnderBrine and determining where items will appear, then selecting the gear icon next to `Copy IDR values` and pasting in the following block into the custom format section:
```json
{bodyName}: {{
    "modelName": {modelName},
    "childName": {childName},
    "localPos": "{localPos.x:5},{localPos.y:5},{localPos.z:5}",
    "localAngles": "{localAngles.x:5},{localAngles.y:5},{localAngles.z:5}",
    "localScale": "{localScale.x:5},{localScale.y:5},{localScale.z:5}"
}}
```
Once done, select `Copy IDR values` and paste the result into the group corresponding to the item you created the display rule for.