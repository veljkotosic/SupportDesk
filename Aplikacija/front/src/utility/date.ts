export function isRealDate(dateInput?: Date | string | null): dateInput is Date | string {
  if (!dateInput) {
    return false
  }

  const date = new Date(dateInput)
  return !Number.isNaN(date.getTime()) && date.getUTCFullYear() > 1
}
