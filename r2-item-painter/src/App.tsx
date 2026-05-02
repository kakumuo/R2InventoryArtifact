import React, { useState } from "react";
import { Sidebar, MainView, PropsList } from "./Components"
import { NotificationPanel } from "./Components/Notificaton/NotificationPanel"
import { useNotificationContext } from "./Components/Notificaton/NotificationService";
import type { DataModelAction } from "./Data";
import { useDataModelContext } from "./Data/DataModelContext";
import { ItemService } from "./Data/ItemService";

export function App() {    
    const {dataModel, } = useDataModelContext(); 
    const notifService = useNotificationContext(); 
    const [lastInitTS, setLastInitTS] = useState(0); 
    React.useEffect(() => {
        (async() => {
            await ItemService.Init();
            setLastInitTS(Date.now())
        })(); 

        return () => ItemService.Dispose(); 
    }, []);
    
    // Handle Undo and Redo
    React.useEffect(() => {
        const HandleUndoRedo = (ev:KeyboardEvent) => {
            let action:DataModelAction|null = null; 
            let didUndo = false; 
            if(ev.key == 'z' && ev.ctrlKey) {
                action = dataModel.StepAction('backward'); 
                didUndo = true; 
            } else if (ev.key == 'y' && ev.ctrlKey) {
                action = dataModel.StepAction('forward'); 
            }

            if(action) {
                notifService.PushNotificaton({
                    title: `${didUndo ? "Undo" : "Redo"}: ${action.Label}`, 
                    body: action.Args, 
                    status: 'info', 
                })
            }
        }; 

        document.addEventListener('keyup', HandleUndoRedo)

        return () => {
            document.removeEventListener('keyup', HandleUndoRedo); 
        }
    }, []); 

    return (
        <div key={lastInitTS} className={styles.container}>
            <Sidebar />
            <MainView />
            <PropsList />
            <NotificationPanel />
        </div>
    )
}

const styles = {
    container: `
        w-screen h-screen
        p-8
        border border-solid
        grid grid-cols-[300px_4fr_300px] grid-rows-1 gap-2
    `, 
}