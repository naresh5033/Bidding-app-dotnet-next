export function numberWithCommas(amount: number) {
    return amount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
} // to format the currency/ bidding amount ex 100,000
