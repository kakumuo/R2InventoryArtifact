### 1.1.0
- Plugin:
    - Replaced logic to update inventory using Inventory Changes; finds delta between previous inventory state and current inventory state
    - Item and Locked slots are painted in groups
    - Temp and consumed items are now moved to `Non-Equip`
    - Temp and consumed items are now not `Droppable`
    - Added default item set to item_data.json
    - InventoryUI now deactivates on `Game Over`
    - Corrupted items in item hold are now properly dropped
    - Item Tooltip label now displays stack count
    - Updated item removal and placement logic to allow for simultaneous item contagion 
- Item Painter:
    - Added missing item icons
    - History is no longer saved to local storage
    - Added File Importin and Exporting

### 1.0.0
- Initial Release