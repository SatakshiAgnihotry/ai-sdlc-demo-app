import { Page, expect } from '@playwright/test';

export class InvoiceApprovalsPage {
  constructor(private readonly page: Page) {}

  async goto() {
    await this.page.goto('/');
  }

  async login(username: string, password: string) {
    await this.page.getByRole('textbox', { name: 'Username' }).fill(username);
    await this.page.getByRole('textbox', { name: 'Password' }).fill(password);
    await this.page.getByRole('button', { name: 'Sign in' }).click();
    await expect(this.page.getByRole('tab', { name: 'Invoices' })).toBeVisible();
  }

  async createInvoice(customerName: string, amount: number) {
    await this.page.getByRole('tab', { name: 'Invoices' }).click();
    await this.page.getByRole('textbox', { name: 'Customer name' }).fill(customerName);
    await this.page.getByRole('spinbutton', { name: 'Amount' }).fill(String(amount));
    await this.page.getByRole('button', { name: 'Create invoice' }).click();
    await expect(this.page.getByRole('cell', { name: customerName })).toBeVisible();
  }

  async getInvoiceId(customerName: string): Promise<string> {
    const row = this.page.getByRole('row').filter({ hasText: customerName });
    const id = await row.getByRole('cell').first().textContent();
    return id?.trim() ?? '';
  }

  async submitForApproval(invoiceId: string) {
    await this.page.getByRole('button', { name: `Submit invoice ${invoiceId} for approval` }).click();
  }

  async goToApprovals() {
    await this.page.getByRole('tab', { name: 'Approvals' }).click();
  }

  async approve(invoiceId: string) {
    await this.page.getByRole('button', { name: `Approve invoice ${invoiceId}` }).click();
  }

  decisionErrorFor(invoiceId: string) {
    return this.page.getByRole('alert', { name: `Decision error for invoice ${invoiceId}` });
  }
}
