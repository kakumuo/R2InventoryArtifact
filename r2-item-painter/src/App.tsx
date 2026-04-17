import { Sidebar, MainView, PropsList } from "./Components"


export function App() {
    return <div className={styles.container}>
        <Sidebar />
        <MainView />
        <PropsList />
    </div>
}

const styles = {
    container: `
        w-screen h-screen
        p-8
        border border-solid
        grid grid-cols-[300px_4fr_300px] grid-rows-1 gap-2
    `, 
}