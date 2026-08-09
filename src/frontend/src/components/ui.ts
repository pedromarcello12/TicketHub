import type { CSSProperties } from 'react'

/* Nocturne CSS token references for inline styles.
   Prefer className="card elev-sm" etc. from index.css where possible. */

export const card: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 'var(--space-2)',
  padding: 'var(--space-4)',
  borderRadius: 'var(--radius-md)',
  background: 'var(--color-surface)',
  boxShadow: 'var(--shadow-sm)',
}

export const table: CSSProperties = { width: '100%', borderCollapse: 'collapse', fontSize: 14 }

export const th: CSSProperties = {
  padding: '8px 12px',
  color: 'color-mix(in srgb, var(--color-text) 60%, transparent)',
  fontWeight: 500,
  fontSize: 11,
  textTransform: 'uppercase',
  letterSpacing: '0.08em',
  textAlign: 'left',
  borderBottom: '1px solid var(--color-divider)',
}

export const td: CSSProperties = {
  padding: '10px 12px',
  color: 'var(--color-text)',
  borderBottom: '1px solid color-mix(in srgb, var(--color-text) 8%, transparent)',
  verticalAlign: 'middle',
}

export const btnPrimary: CSSProperties = {
  display: 'inline-flex', alignItems: 'center', gap: 6,
  cursor: 'pointer', fontSize: 14, fontFamily: 'var(--font-heading)',
  color: 'var(--color-accent)', background: 'transparent',
  border: '1px solid var(--color-accent)', padding: '6px 14px',
  borderRadius: 'var(--radius-md)',
}

export const btnSecondary: CSSProperties = {
  display: 'inline-flex', alignItems: 'center', gap: 6,
  cursor: 'pointer', fontSize: 14, fontFamily: 'var(--font-heading)',
  color: 'var(--color-text)', background: 'transparent',
  border: '1px solid var(--color-divider)', padding: '6px 14px',
  borderRadius: 'var(--radius-md)',
}

export const btnDanger: CSSProperties = {
  display: 'inline-flex', alignItems: 'center', gap: 6,
  cursor: 'pointer', fontSize: 13,
  color: 'var(--color-danger)', background: 'transparent',
  border: '1px solid var(--color-danger)', padding: '5px 10px',
  borderRadius: 'var(--radius-md)',
}

export const btnSmall: CSSProperties = {
  display: 'inline-flex', alignItems: 'center', gap: 4,
  cursor: 'pointer', fontSize: 12, color: 'var(--color-text)',
  background: 'color-mix(in srgb, var(--color-text) 8%, transparent)',
  border: 'none', padding: '4px 10px', borderRadius: 'var(--radius-sm)',
}

export const labelStyle: CSSProperties = {
  display: 'flex', flexDirection: 'column', gap: 5,
  fontSize: 12, color: 'color-mix(in srgb, var(--color-text) 70%, transparent)',
}

export const inputStyle: CSSProperties = {
  width: '100%', minHeight: 36, padding: '6px 10px', fontSize: 14,
  color: 'var(--color-text)', background: 'var(--color-surface)',
  border: '1px solid var(--color-divider)', borderRadius: 'var(--radius-md)',
}

export const pageHeader: CSSProperties = {
  display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 24,
}

export const h1: CSSProperties = {
  margin: 0, fontSize: 28, fontFamily: 'var(--font-heading)',
  fontWeight: 500, color: 'var(--color-text)', letterSpacing: '-0.015em',
}
