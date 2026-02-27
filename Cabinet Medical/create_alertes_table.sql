-- Script SQL pour créer la table Alertes
-- Ce script peut être exécuté directement dans SQL Server Management Studio

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Alertes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Alertes] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Type] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [UserId] int NOT NULL,
        [RendezVousId] int NULL,
        [EstLue] bit NOT NULL DEFAULT 0,
        [DateCreation] datetime2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Alertes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Alertes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Alertes_RendezVous_RendezVousId] FOREIGN KEY ([RendezVousId]) REFERENCES [dbo].[RendezVous] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_Alertes_UserId] ON [dbo].[Alertes] ([UserId]);
    CREATE INDEX [IX_Alertes_RendezVousId] ON [dbo].[Alertes] ([RendezVousId]);

    -- Ajouter la migration à l'historique
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251216201313_AddAlertesTable', '10.0.1');

    PRINT 'Table Alertes créée avec succès!';
END
ELSE
BEGIN
    PRINT 'La table Alertes existe déjà.';
END

