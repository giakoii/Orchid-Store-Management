// Utility functions for formatting

/**
 * Format currency to Vietnamese Dong (VND) with space-separated thousands
 * @param amount - The amount to format
 * @returns Formatted string like "5 000 000 VNĐ"
 */
export const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'decimal',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(amount).replace(/,/g, ' ') + ' VNĐ';
};

/**
 * Format number with space-separated thousands
 * @param number - The number to format
 * @returns Formatted string like "5 000 000"
 */
export const formatNumber = (number: number): string => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'decimal',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(number).replace(/,/g, ' ');
};
