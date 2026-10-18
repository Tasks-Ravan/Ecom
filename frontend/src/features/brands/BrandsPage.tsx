import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Boxes, Pencil, Plus, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'sonner';
import { createBrand, deleteBrand, listBrands, updateBrand } from '../../api/brands';
import { ApiError } from '../../api/client';
import type { BrandRequest, BrandResponse } from '../../api/types';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { EmptyState, ErrorState, LoadingState } from '../../components/States';
import { PageHeader } from '../../components/PageHeader';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Dialog, DialogContent } from '../../components/ui/dialog';
import { Input } from '../../components/ui/input';
import { safeImageUrl } from '../../lib/utils';
import { useTranslation } from 'react-i18next';

const brandSchema = z.object({
  name: z.string().trim().min(1).max(150),
  logoUrl: z.string().refine((value) => !value || (URL.canParse(value) && ['http:', 'https:'].includes(new URL(value).protocol))),
  isActive: z.boolean(),
});
type BrandValues = z.infer<typeof brandSchema>;

export function BrandsPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [dialog, setDialog] = useState<{ open: boolean; brand?: BrandResponse }>({ open: false });
  const [deleteTarget, setDeleteTarget] = useState<BrandResponse | null>(null);
  const query = useQuery({ queryKey: ['brands'], queryFn: listBrands });
  const save = useMutation({ mutationFn: ({ values, brand }: { values: BrandRequest; brand?: BrandResponse }) => brand ? updateBrand(brand.id, values) : createBrand(values),
    onSuccess: async () => { toast.success(dialog.brand ? t('common.updated') : t('common.created')); setDialog({ open: false }); await queryClient.invalidateQueries({ queryKey: ['brands'] }); },
    onError: (error) => toast.error(error instanceof ApiError ? error.message : t('errors.request')) });
  const remove = useMutation({ mutationFn: deleteBrand, onSuccess: async () => { toast.success(t('common.deleted')); setDeleteTarget(null); await queryClient.invalidateQueries({ queryKey: ['brands'] }); }, onError: (error) => toast.error(error instanceof ApiError ? error.message : t('errors.request')) });
  if (query.isPending) return <><PageHeader title={t('admin.brands')} description={t('admin.brandsDescription')} /><LoadingState rows={5} /></>;
  if (query.isError) return <ErrorState onRetry={() => void query.refetch()} />;
  return <>
    <PageHeader title={t('admin.brands')} description={t('admin.brandsDescription')} action={<Button onClick={() => setDialog({ open: true })}><Plus size={16} />{t('admin.createBrand')}</Button>} />
    {query.data.length ? <section className="brand-grid">{query.data.map((brand) => <BrandCard key={brand.id} brand={brand} onEdit={() => setDialog({ open: true, brand })} onDelete={() => setDeleteTarget(brand)} />)}</section>
      : <section className="card"><EmptyState title={t('admin.noBrands')} action={<Button onClick={() => setDialog({ open: true })}><Plus size={15} />{t('admin.createBrand')}</Button>} /></section>}
    <BrandDialog open={dialog.open} brand={dialog.brand} busy={save.isPending} onOpenChange={(open) => setDialog((current) => ({ ...current, open }))}
      onSubmit={(values) => save.mutate({ values, brand: dialog.brand })} />
    <ConfirmDialog open={Boolean(deleteTarget)} onOpenChange={(open) => !open && setDeleteTarget(null)} title={t('admin.deleteBrand')} description={t('admin.deleteBrandConfirm')} busy={remove.isPending} onConfirm={() => deleteTarget && remove.mutate(deleteTarget.id)} />
  </>;
}

function BrandCard({ brand, onEdit, onDelete }: { brand: BrandResponse; onEdit: () => void; onDelete: () => void }) {
  const { t } = useTranslation();
  const image = safeImageUrl(brand.logoUrl);
  return <article className="brand-card card"><div className="brand-card-top"><span className="brand-logo">{image ? <img src={image} alt={t('admin.brandLogo', { name: brand.name })} /> : <Boxes size={25} />}</span>
      <div className="table-actions"><button className="icon-button" onClick={onEdit} aria-label={t('common.edit')}><Pencil size={16} /></button><button className="icon-button danger-icon" onClick={onDelete} aria-label={t('common.delete')}><Trash2 size={16} /></button></div></div>
    <h2>{brand.name}</h2><p>{brand.slug}</p><div className="brand-card-footer"><Badge className={brand.isActive ? 'status-active' : 'status-inactive'}>{brand.isActive ? t('common.active') : t('common.inactive')}</Badge><span>{brand.logoUrl ? t('admin.logoAdded') : t('admin.noLogo')}</span></div></article>;
}

function BrandDialog({ open, brand, busy, onOpenChange, onSubmit }: { open: boolean; brand?: BrandResponse; busy: boolean; onOpenChange: (open: boolean) => void; onSubmit: (values: BrandRequest) => void }) {
  const { t } = useTranslation();
  const { register, handleSubmit, reset, watch, setValue, formState: { errors } } = useForm<BrandValues>({ resolver: zodResolver(brandSchema), defaultValues: { name: '', logoUrl: '', isActive: true } });
  useEffect(() => { reset({ name: brand?.name ?? '', logoUrl: brand?.logoUrl ?? '', isActive: brand?.isActive ?? true }); }, [brand, open, reset]);
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent title={brand ? t('admin.editBrand') : t('admin.createBrand')} description={t('admin.brandDialogHint')}>
    <form className="form-stack dialog-form" onSubmit={(event) => void handleSubmit((values) => onSubmit({ ...values, logoUrl: values.logoUrl || null }))(event)} noValidate>
      <label className="form-field"><span className="field-label">{t('common.name')}</span><Input {...register('name')} maxLength={150} />{errors.name && <span className="form-error">{errors.name.message}</span>}</label>
      <label className="form-field"><span className="field-label">{t('admin.logoUrl')}</span><Input type="url" placeholder={t('admin.logoPlaceholder')} {...register('logoUrl')} />{errors.logoUrl && <span className="form-error">{errors.logoUrl.message}</span>}</label>
      <label className="switch-row"><span><strong>{t('common.active')}</strong><small>{t('admin.brandActiveHint')}</small></span><input type="checkbox" role="switch" checked={watch('isActive')} onChange={(event) => setValue('isActive', event.target.checked)} /></label>
      <div className="flex justify-end gap-3 mt-3"><Button type="button" variant="secondary" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button type="submit" disabled={busy}>{busy ? t('common.loading') : t('admin.saveBrand')}</Button></div>
    </form>
  </DialogContent></Dialog>;
}
