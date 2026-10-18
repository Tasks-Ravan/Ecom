import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ChevronDown, ChevronRight, FolderTree, Pencil, Plus, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'sonner';
import { createCategory, deleteCategory, listCategories, updateCategory } from '../../api/categories';
import { ApiError } from '../../api/client';
import type { CategoryRequest, CategoryResponse } from '../../api/types';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { EmptyState, ErrorState, LoadingState } from '../../components/States';
import { PageHeader } from '../../components/PageHeader';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Dialog, DialogContent } from '../../components/ui/dialog';
import { Input } from '../../components/ui/input';
import { useTranslation } from 'react-i18next';

const categorySchema = z.object({ name: z.string().trim().min(1).max(150), parentCategoryId: z.string().nullable(), isActive: z.boolean() });
type CategoryValues = z.infer<typeof categorySchema>;

export function CategoriesPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [dialog, setDialog] = useState<{ open: boolean; category?: CategoryResponse }>({ open: false });
  const [deleteTarget, setDeleteTarget] = useState<CategoryResponse | null>(null);
  const query = useQuery({ queryKey: ['categories'], queryFn: listCategories });
  const save = useMutation({
    mutationFn: ({ values, category }: { values: CategoryRequest; category?: CategoryResponse }) => category ? updateCategory(category.id, values) : createCategory(values),
    onSuccess: async () => { toast.success(dialog.category ? t('common.updated') : t('common.created')); setDialog({ open: false }); await queryClient.invalidateQueries({ queryKey: ['categories'] }); },
    onError: (error) => toast.error(error instanceof ApiError ? error.message : t('errors.request')),
  });
  const remove = useMutation({ mutationFn: deleteCategory, onSuccess: async () => { toast.success(t('common.deleted')); setDeleteTarget(null); await queryClient.invalidateQueries({ queryKey: ['categories'] }); }, onError: (error) => toast.error(error instanceof ApiError ? error.message : t('errors.request')) });
  if (query.isPending) return <><PageHeader title={t('admin.categories')} description={t('admin.categoriesDescription')} /><LoadingState rows={6} /></>;
  if (query.isError) return <ErrorState onRetry={() => void query.refetch()} />;
  const categories = query.data;
  return <>
    <PageHeader title={t('admin.categories')} description={t('admin.categoriesDescription')} action={<Button onClick={() => setDialog({ open: true })}><Plus size={16} />{t('admin.createCategory')}</Button>} />
    <section className="card tree-card">
      <div className="table-toolbar tree-toolbar"><div><span className="card-title-small">{t('admin.categoryStructure')}</span><p className="muted text-sm">{categories.length} {t('admin.categories').toLocaleLowerCase()}</p></div><span className="tree-total"><FolderTree size={16} />{categories.filter((item) => item.parentCategoryId === null).length} {t('admin.topLevel')}</span></div>
      {categories.length ? <div className="category-tree">{categories.filter((item) => item.parentCategoryId === null).sort((a, b) => a.name.localeCompare(b.name)).map((category) =>
        <CategoryNode key={category.id} category={category} categories={categories} onEdit={(item) => setDialog({ open: true, category: item })} onDelete={setDeleteTarget} />)}</div> : <EmptyState title={t('admin.noCategories')} action={<Button onClick={() => setDialog({ open: true })}><Plus size={15} />{t('admin.createCategory')}</Button>} />}
    </section>
    <CategoryDialog open={dialog.open} category={dialog.category} categories={categories} busy={save.isPending} onOpenChange={(open) => setDialog((current) => ({ ...current, open }))}
      onSubmit={(values) => save.mutate({ values, category: dialog.category })} />
    <ConfirmDialog open={Boolean(deleteTarget)} onOpenChange={(open) => !open && setDeleteTarget(null)} title={t('admin.deleteCategory')} description={t('admin.deleteCategoryConfirm')} busy={remove.isPending} onConfirm={() => deleteTarget && remove.mutate(deleteTarget.id)} />
  </>;
}

function CategoryNode({ category, categories, onEdit, onDelete, depth = 0, visited = new Set<string>() }: {
  category: CategoryResponse; categories: CategoryResponse[]; onEdit: (item: CategoryResponse) => void; onDelete: (item: CategoryResponse) => void; depth?: number; visited?: Set<string>;
}) {
  const { t } = useTranslation();
  const [expanded, setExpanded] = useState(true);
  if (visited.has(category.id)) return null;
  const nextVisited = new Set(visited).add(category.id);
  const children = categories.filter((item) => item.parentCategoryId === category.id).sort((a, b) => a.name.localeCompare(b.name));
  return <div className="category-node-wrap"><div className="category-node" style={{ paddingLeft: `${16 + depth * 28}px` }}>
    <button className="tree-disclosure" onClick={() => setExpanded((value) => !value)} disabled={!children.length} aria-label={expanded ? t('admin.collapse') : t('admin.expand')}>
      {children.length ? expanded ? <ChevronDown size={16} /> : <ChevronRight size={16} /> : <span className="tree-leaf" />}</button>
    <span className="tree-folder"><FolderTree size={17} /></span><div className="tree-item-copy"><strong>{category.name}</strong><small>/{category.slug}</small></div>
    <Badge className={category.isActive ? 'status-active' : 'status-inactive'}>{category.isActive ? t('common.active') : t('common.inactive')}</Badge>
    <span className="tree-child-count">{children.length ? `${children.length} ${t('admin.children')}` : ''}</span>
    <div className="table-actions"><button className="icon-button" aria-label={t('common.edit')} onClick={() => onEdit(category)}><Pencil size={16} /></button><button className="icon-button danger-icon" aria-label={t('common.delete')} onClick={() => onDelete(category)}><Trash2 size={16} /></button></div>
  </div>{expanded && children.map((child) => <CategoryNode key={child.id} category={child} categories={categories} onEdit={onEdit} onDelete={onDelete} depth={depth + 1} visited={nextVisited} />)}</div>;
}

function CategoryDialog({ open, category, categories, busy, onOpenChange, onSubmit }: {
  open: boolean; category?: CategoryResponse; categories: CategoryResponse[]; busy: boolean; onOpenChange: (open: boolean) => void; onSubmit: (values: CategoryRequest) => void;
}) {
  const { t } = useTranslation();
  const { register, handleSubmit, reset, watch, setValue, formState: { errors } } = useForm<CategoryValues>({
    resolver: zodResolver(categorySchema), defaultValues: { name: category?.name ?? '', parentCategoryId: category?.parentCategoryId ?? null, isActive: category?.isActive ?? true },
  });
  useEffect(() => { reset({ name: category?.name ?? '', parentCategoryId: category?.parentCategoryId ?? null, isActive: category?.isActive ?? true }); }, [category, open, reset]);
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent title={category ? t('admin.editCategory') : t('admin.createCategory')} description={t('admin.categoryDialogHint')}>
    <form className="form-stack dialog-form" onSubmit={(event) => void handleSubmit((values) => onSubmit(values as CategoryRequest))(event)} noValidate>
      <label className="form-field"><span className="field-label">{t('common.name')}</span><Input {...register('name')} maxLength={150} />{errors.name && <span className="form-error">{errors.name.message}</span>}</label>
      <label className="form-field"><span className="field-label">{t('admin.parentCategory')}</span><select className="input select-native" value={watch('parentCategoryId') ?? ''} onChange={(event) => setValue('parentCategoryId', event.target.value || null)}>
        <option value="">{t('admin.rootCategory')}</option>{categories.filter((item) => item.id !== category?.id).map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label className="switch-row"><span><strong>{t('common.active')}</strong><small>{t('admin.categoryActiveHint')}</small></span><input type="checkbox" role="switch" checked={watch('isActive')} onChange={(event) => setValue('isActive', event.target.checked)} /></label>
      <div className="flex justify-end gap-3 mt-3"><Button type="button" variant="secondary" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button type="submit" disabled={busy}>{busy ? t('common.loading') : t('admin.saveCategory')}</Button></div>
    </form>
  </DialogContent></Dialog>;
}
