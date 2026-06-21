export function stringToColor(str: string): string {
  let hash = 5381
  for (let i = 0; i < str.length; i++) {
    hash = ((hash << 5) + hash + str.charCodeAt(i)) & 0xffffffff
  }
  return `hsl(${Math.abs(hash) % 360}, 65%, 55%)`
}

