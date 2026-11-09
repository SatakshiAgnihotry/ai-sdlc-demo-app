import { test, expect } from '@playwright/test';
import { InvoiceApprovalsPage } from '../pages/invoice-approvals-page';

test('a Manager approving an invoice over their limit sees the decline reason inline', async ({ page }) => {
  const invoices = new InvoiceApprovalsPage(page);
  const customerName = `E2E Over Limit ${Date.now()}`;

  await invoices.goto();
  await invoices.login('demo.manager', 'ChangeMe123!');
  await invoices.createInvoice(customerName, 80_000);

  const invoiceId = await invoices.getInvoiceId(customerName);
  await invoices.submitForApproval(invoiceId);

  await invoices.goToApprovals();
  await invoices.approve(invoiceId);

  await expect(invoices.decisionErrorFor(invoiceId)).toHaveText(
    'A Manager cannot approve invoices over 50,000; this invoice is 80,000.'
  );
});
