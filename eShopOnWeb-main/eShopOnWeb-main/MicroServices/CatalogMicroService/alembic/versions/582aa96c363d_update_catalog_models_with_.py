"""Update Catalog Models with configurations 3

Revision ID: 582aa96c363d
Revises: e7a12647051d
Create Date: 2025-09-05 12:21:42.474577

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision: str = '582aa96c363d'
down_revision: Union[str, Sequence[str], None] = 'e7a12647051d'
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    """Upgrade schema."""
    # Alter the 'catalogtype' table to set the length of 'type' to 100
    with op.batch_alter_table('catalogtype', schema=None) as batch_op:
        batch_op.alter_column('type',
               existing_type=sa.String(length=255),
               type_=sa.String(length=100),
               existing_nullable=False)

    # Alter the 'catalogbrand' table to set the length of 'brand' to 100
    with op.batch_alter_table('catalogbrand', schema=None) as batch_op:
        batch_op.alter_column('brand',
               existing_type=sa.String(length=255),
               type_=sa.String(length=100),
               existing_nullable=False)


def downgrade() -> None:
    """Downgrade schema."""
    # Revert the 'catalogtype' column length back to 255
    with op.batch_alter_table('catalogtype', schema=None) as batch_op:
        batch_op.alter_column('type',
               existing_type=sa.String(length=100),
               type_=sa.String(length=255),
               existing_nullable=False)

    # Revert the 'catalogbrand' column length back to 255
    with op.batch_alter_table('catalogbrand', schema=None) as batch_op:
        batch_op.alter_column('brand',
               existing_type=sa.String(length=100),
               type_=sa.String(length=255),
               existing_nullable=False)
