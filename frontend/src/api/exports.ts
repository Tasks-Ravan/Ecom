import { apiRequest } from './client';
import type { ExportProductsToMailRequest, ExportProductsToMailResponse } from './types';

export const sendProductExport = (request: ExportProductsToMailRequest) =>
  apiRequest<ExportProductsToMailResponse>('/products/export/send-mail', {
    method: 'POST',
    body: JSON.stringify(request),
  });
