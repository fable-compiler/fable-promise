export async function* asyncGenerator() {
    let i = 0;
    while (i < 5) {
        yield i++;
    }
}

/**
 * 
 * @param {AsyncIterable<String>} iterable 
 */
export async function handleAsyncIterable(iterable) {
    let acc = '';
    for await (const value of iterable) {
        acc = value + acc;
    }
    return acc;
}

/**
 * 
 * @param {AsyncIterable<String>} iterable 
 */
export async function handleAsyncIterableWithBreak(iterable) {
    let i = 0;
    let acc = '';
    for await (const value of iterable) {
        if (i === 2) {
            break;
        }
        i++;
        acc = value + acc;
    }
    return acc;
}