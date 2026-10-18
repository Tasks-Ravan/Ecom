import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { ArrowDownToLine, ArrowUpRight, BriefcaseBusiness, Clock3, Mail, Send } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'sonner';
import { downloadProductExcel } from '../../api/products';
import { sendProductExport } from '../../api/exports';
import { ApiError } from '../../api/client';
import { PageHeader } from '../../components/PageHeader';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { useTranslation } from 'react-i18next';

const exportSchema = z.object({ recipientEmail: z.email().max(256) });
type ExportFormValues = z.infer<typeof exportSchema>;

export function ExportsPage() {
  const { t } = useTranslation();
  const { register, handleSubmit, reset, formState: { errors } } = useForm<ExportFormValues>({ resolver: zodResolver(exportSchema), defaultValues: { recipientEmail: '' } });
  const download = useMutation({ mutationFn: downloadProductExcel, onSuccess: (blob) => {
    const url = URL.createObjectURL(blob); const anchor = document.createElement('a');
    anchor.href = url; anchor.download = 'products.xlsx'; anchor.click(); URL.revokeObjectURL(url); toast.success(t('admin.downloadReady'));
  }, onError: (error) => toast.error(error instanceof ApiError ? error.message : t('errors.request')) });
  const send = useMutation({ mutationFn: (values: ExportFormValues) => sendProductExport(values), onSuccess: (response) => {
    toast.success(`${t('admin.queued')} · ${response.jobId}`, { description: t('admin.queueNote') }); reset();
  }, onError: (error) => toast.error(error instanceof ApiError ? error.message : t('errors.request')) });
  return <>
    <PageHeader title={t('admin.exportsTitle')} description={t('admin.exportsBody')} />
    <div className="export-grid">
      <section className="card export-card"><span className="export-icon lavender"><ArrowDownToLine size={20} /></span><div><h2>{t('admin.downloadTitle')}</h2><p>{t('admin.downloadBody')}</p></div>
        <Button variant="secondary" disabled={download.isPending} onClick={() => download.mutate()}>{download.isPending ? t('common.loading') : t('admin.downloadExcel')} <ArrowDownToLine size={16} /></Button>
      </section>
      <section className="card export-card"><span className="export-icon mint"><Mail size={20} /></span><div><h2>{t('admin.sendExportTitle')}</h2><p>{t('admin.sendExportBody')}</p></div>
        <form className="export-form" onSubmit={(event) => void handleSubmit((values) => send.mutate(values))(event)} noValidate>
          <label className="form-field"><span className="field-label">{t('admin.recipient')}</span><Input type="email" autoComplete="email" placeholder={t('admin.recipientPlaceholder')} {...register('recipientEmail')} />{errors.recipientEmail && <span className="form-error">{errors.recipientEmail.message}</span>}</label>
          <Button type="submit" disabled={send.isPending}>{send.isPending ? t('common.loading') : t('admin.queueExport')} <Send size={15} /></Button>
        </form>
      </section>
    </div>
    <section className="card schedule-card"><span className="schedule-icon"><Clock3 size={20} /></span><div><h2>{t('admin.scheduleTitle')}</h2><p>{t('admin.scheduleBody')}</p></div>
      <a className="button button-secondary button-default" href="/hangfire" target="_blank" rel="noreferrer">{t('admin.hangfire')}<ArrowUpRight size={15} /></a>
    </section>
    <div className="export-footnote"><BriefcaseBusiness size={15} /><span>{t('admin.exportFootnote')}</span></div>
  </>;
}
