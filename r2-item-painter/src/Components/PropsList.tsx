import { Input } from "./Components"

export function PropsList() {
    return <div className={styles._}>

        <div className={styles.header._}>
            <h2>Properties</h2>
        </div>

        <div className={styles.propList}>
            <PropItem />
        </div>
    </div>
}

export function PropItem() {
    return <div className={styles.propItem._}>
        <p>Some Title</p>
        <Input />
    </div>
}

const styles = {
    _: `border`, 
    header: {
        _: `border p-2`, 
    },
    propList: `p-2`, 
    propItem:{
        _:`grid grid-cols-1 grid-rows-2`
    }
}