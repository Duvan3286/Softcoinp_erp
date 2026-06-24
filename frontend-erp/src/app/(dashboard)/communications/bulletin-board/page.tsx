'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Plus, Pencil, Trash2, Pin, PinOff, Search } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import communicationService, { BulletinBoardPost, BulletinBoardPostAdmin } from '@/lib/communication-service';
import axios from 'axios';

const categoryLabels: Record<string, string> = {
  Administrative: 'Administrativo',
  Financial: 'Financiero',
  LivingTogether: 'Convivencia',
  Events: 'Eventos',
  Urgent: 'Urgente',
};

const categoryBadgeClass: Record<string, string> = {
  Administrative: 'badge-neutral',
  Financial: 'badge-info',
  LivingTogether: 'badge-success',
  Events: 'badge-warning',
  Urgent: 'badge-danger',
};

export default function BulletinBoardPage() {
  const [posts, setPosts] = useState<BulletinBoardPostAdmin[]>([]);
  const [activePosts, setActivePosts] = useState<BulletinBoardPost[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [view, setView] = useState<'active' | 'admin'>('active');
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [includeArchived, setIncludeArchived] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  // Form state
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [category, setCategory] = useState('Administrative');
  const [isPinned, setIsPinned] = useState(false);
  const [expiresAt, setExpiresAt] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchPosts();
  }, [view, includeArchived]);

  const fetchPosts = async () => {
    setLoading(true);
    setError('');
    try {
      if (view === 'active') {
        const data = await communicationService.getActiveBulletinPosts();
        setActivePosts(data);
      } else {
        const data = await communicationService.getAllBulletinPosts(includeArchived);
        setPosts(data);
      }
    } catch {
      setError('Error al cargar publicaciones.');
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setTitle('');
    setContent('');
    setCategory('Administrative');
    setIsPinned(false);
    setExpiresAt('');
    setEditingId(null);
    setShowForm(false);
  };

  const handleEdit = (post: BulletinBoardPostAdmin) => {
    setTitle(post.title);
    setContent(post.content);
    setCategory(post.category);
    setIsPinned(post.isPinned);
    setExpiresAt(post.expiresAt ? post.expiresAt.split('T')[0] : '');
    setEditingId(post.id);
    setShowForm(true);
  };

  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      if (editingId) {
        await communicationService.updateBulletinPost(editingId, {
          title,
          content,
          category,
          isPinned,
          expiresAt: expiresAt || null,
        });
      } else {
        await communicationService.createBulletinPost({
          title,
          content,
          category,
          isPinned,
          expiresAt: expiresAt || null,
        });
      }
      resetForm();
      fetchPosts();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al guardar la publicación.');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Archivar esta publicación?')) return;
    try {
      await communicationService.archiveBulletinPost(id);
      fetchPosts();
    } catch {
      setError('Error al archivar.');
    }
  };

  const filteredPosts = posts.filter((p) =>
    p.title.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">Cartelera Digital</h1>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setView('active')}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              view === 'active'
                ? 'bg-emerald-600 text-white'
                : 'bg-muted text-muted-foreground hover:bg-muted/80'
            }`}
          >
            Cartelera
          </button>
          <button
            onClick={() => setView('admin')}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              view === 'admin'
                ? 'bg-emerald-600 text-white'
                : 'bg-muted text-muted-foreground hover:bg-muted/80'
            }`}
          >
            Administrar
          </button>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      {view === 'active' ? (
        <div className="space-y-4">
          {activePosts.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">
              <p>No hay publicaciones activas en la cartelera.</p>
            </div>
          ) : (
            activePosts.map((post) => (
              <Card key={post.id} className={`${post.isPinned ? 'border-emerald-400 dark:border-emerald-600 ring-1 ring-emerald-400/20' : ''}`}>
                <CardContent className="p-6">
                  <div className="flex items-start gap-3">
                    {post.isPinned && (
                      <Pin className="w-5 h-5 text-emerald-600 flex-shrink-0 mt-1" />
                    )}
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <span className={`badge ${categoryBadgeClass[post.category] || 'badge-neutral'}`}>
                          {categoryLabels[post.category] || post.category}
                        </span>
                        <span className="text-xs text-muted-foreground">
                          {new Date(post.publishedAt).toLocaleDateString('es-CO', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                          })}
                        </span>
                      </div>
                      <h3 className="text-lg font-bold text-foreground mb-2">{post.title}</h3>
                      <div className="prose prose-sm dark:prose-invert max-w-none whitespace-pre-wrap text-muted-foreground">
                        {post.content}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div>
      ) : (
        <>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2 bg-muted rounded-lg px-3 py-2">
                <Search className="w-4 h-4 text-muted-foreground" />
                <input
                  type="text"
                  placeholder="Buscar..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="bg-transparent border-none outline-none text-sm flex-1"
                />
              </div>
              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input
                  type="checkbox"
                  checked={includeArchived}
                  onChange={(e) => setIncludeArchived(e.target.checked)}
                  className="rounded border-emerald-600/30 text-emerald-600"
                />
                Incluir archivadas
              </label>
            </div>
            <Button onClick={() => { resetForm(); setShowForm(true); }}>
              <Plus className="w-4 h-4 mr-2" />
              Nueva Publicación
            </Button>
          </div>

          {showForm && (
            <Card>
              <CardContent className="p-6 space-y-4">
                <h2 className="text-lg font-semibold text-foreground">
                  {editingId ? 'Editar Publicación' : 'Nueva Publicación'}
                </h2>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Título</label>
                  <input
                    type="text"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Contenido</label>
                  <textarea
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background min-h-[150px]"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Categoría</label>
                    <select
                      value={category}
                      onChange={(e) => setCategory(e.target.value)}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-background"
                    >
                      {Object.entries(categoryLabels).map(([key, label]) => (
                        <option key={key} value={key}>{label}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Vence el</label>
                    <input
                      type="date"
                      value={expiresAt}
                      onChange={(e) => setExpiresAt(e.target.value)}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                    />
                  </div>
                </div>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={isPinned}
                    onChange={(e) => setIsPinned(e.target.checked)}
                    className="rounded border-emerald-600/30 text-emerald-600 focus:ring-emerald-600"
                  />
                  <span className="text-sm">Fijar al tope de la cartelera</span>
                </label>

                <div className="flex gap-2">
                  <Button onClick={handleSave} disabled={saving || !title}>
                    {saving ? 'Guardando...' : editingId ? 'Actualizar' : 'Publicar'}
                  </Button>
                  <Button variant="secondary" onClick={resetForm}>Cancelar</Button>
                </div>
              </CardContent>
            </Card>
          )}

          <div className="space-y-3">
            {filteredPosts.length === 0 ? (
              <p className="text-muted-foreground text-center py-8">No hay publicaciones.</p>
            ) : (
              filteredPosts.map((post) => (
                <Card key={post.id}>
                  <CardContent className="p-4">
                    <div className="flex items-start justify-between">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <span className={`badge ${categoryBadgeClass[post.category] || 'badge-neutral'}`}>
                            {categoryLabels[post.category] || post.category}
                          </span>
                          {post.isPinned && <PinOff className="w-3 h-3 text-emerald-600" />}
                          {post.isDeleted && <span className="badge badge-danger">Archivada</span>}
                        </div>
                        <h3 className="font-semibold text-foreground">{post.title}</h3>
                        <p className="text-xs text-muted-foreground mt-1">
                          {new Date(post.publishedAt).toLocaleDateString('es-CO')}
                          {post.expiresAt && ` — Vence: ${new Date(post.expiresAt).toLocaleDateString('es-CO')}`}
                        </p>
                      </div>
                      <div className="flex items-center gap-1 ml-2">
                        <button
                          onClick={() => handleEdit(post)}
                          className="p-1.5 rounded-lg hover:bg-muted transition-colors"
                          title="Editar"
                        >
                          <Pencil className="w-4 h-4 text-muted-foreground" />
                        </button>
                        {!post.isDeleted && (
                          <button
                            onClick={() => handleDelete(post.id)}
                            className="p-1.5 rounded-lg hover:bg-red-50 dark:hover:bg-red-950/20 transition-colors"
                            title="Archivar"
                          >
                            <Trash2 className="w-4 h-4 text-red-500" />
                          </button>
                        )}
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))
            )}
          </div>
        </>
      )}
    </div>
  );
}
