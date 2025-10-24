import { test, expect } from '@playwright/test';

test.describe('Generate Records Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/generate');
  });

  test('should display the page title', async ({ page }) => {
    await expect(page.locator('h2')).toHaveText('Generate Records');
  });

  test('should have default value of 10 in input field', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    await expect(input).toHaveValue('10');
  });

  test('should generate records successfully with valid input', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    const button = page.getByTestId('generate-button');

    // Clear and enter a new value
    await input.clear();
    await input.fill('5');

    // Click generate button
    await button.click();

    // Wait for results to appear
    await expect(page.getByTestId('results-container')).toBeVisible();

    // Verify the number of results
    const resultItems = page.getByTestId('result-item');
    await expect(resultItems).toHaveCount(5);

    // Verify the format of the first item
    const firstItem = resultItems.first();
    await expect(firstItem).toContainText('1: Name 1');
  });

  test('should display error for invalid input (below minimum)', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    const button = page.getByTestId('generate-button');

    // Enter invalid value
    await input.clear();
    await input.fill('0');

    // Click generate button
    await button.click();

    // Wait for error message to appear
    await expect(page.getByTestId('error-message')).toBeVisible();
    await expect(page.getByTestId('error-message')).toContainText('must be between 1 and 1000');
  });

  test('should display error for invalid input (above maximum)', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    const button = page.getByTestId('generate-button');

    // Enter invalid value
    await input.clear();
    await input.fill('1001');

    // Click generate button
    await button.click();

    // Wait for error message to appear
    await expect(page.getByTestId('error-message')).toBeVisible();
    await expect(page.getByTestId('error-message')).toContainText('must be between 1 and 1000');
  });

  test('should generate maximum allowed records (1000)', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    const button = page.getByTestId('generate-button');

    // Enter maximum value
    await input.clear();
    await input.fill('1000');

    // Click generate button
    await button.click();

    // Wait for results to appear
    await expect(page.getByTestId('results-container')).toBeVisible({ timeout: 10000 });

    // Verify the number of results
    const resultItems = page.getByTestId('result-item');
    await expect(resultItems).toHaveCount(1000);
  });

  test('should generate minimum allowed records (1)', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    const button = page.getByTestId('generate-button');

    // Enter minimum value
    await input.clear();
    await input.fill('1');

    // Click generate button
    await button.click();

    // Wait for results to appear
    await expect(page.getByTestId('results-container')).toBeVisible();

    // Verify the number of results
    const resultItems = page.getByTestId('result-item');
    await expect(resultItems).toHaveCount(1);
    await expect(resultItems.first()).toContainText('1: Name 1');
  });

  test('should clear previous results when generating new records', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    const button = page.getByTestId('generate-button');

    // First generation
    await input.clear();
    await input.fill('3');
    await button.click();
    await expect(page.getByTestId('results-container')).toBeVisible();
    
    let resultItems = page.getByTestId('result-item');
    await expect(resultItems).toHaveCount(3);

    // Second generation with different number
    await input.clear();
    await input.fill('5');
    await button.click();
    await expect(page.getByTestId('results-container')).toBeVisible();
    
    resultItems = page.getByTestId('result-item');
    await expect(resultItems).toHaveCount(5);
  });

  test('should clear error when generating valid records after error', async ({ page }) => {
    const input = page.getByTestId('number-of-records-input');
    const button = page.getByTestId('generate-button');

    // First, generate an error
    await input.clear();
    await input.fill('0');
    await button.click();
    await expect(page.getByTestId('error-message')).toBeVisible();

    // Then, generate valid records
    await input.clear();
    await input.fill('2');
    await button.click();
    
    // Error should be gone and results should appear
    await expect(page.getByTestId('error-message')).not.toBeVisible();
    await expect(page.getByTestId('results-container')).toBeVisible();
  });
});
