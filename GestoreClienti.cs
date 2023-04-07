using ClientiLibrary;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
//riferimenti database
using MySql.Data.MySqlClient;
using System.Configuration;


namespace AssemblyGestore
{
    public class GestoreClienti : IGestoreC
    {
        private string _connectionDB;

        // Costruttore che accetta il percorso come argomento
        public GestoreClienti(string connectionDB)
        {
            _connectionDB = connectionDB;
        }

        // CERCA //
        public List<Cliente> CercaCliente(string parametroRicerca, string scelta)
        {
            List<Cliente> clientiTrovati = new List<Cliente>();  // Crea una nuova lista vuota per memorizzare i clienti trovati

            using (MySqlConnection connection = new MySqlConnection(_connectionDB))
            {
                connection.Open();
                string query = $"SELECT * FROM Clienti WHERE {scelta} = @parametroRicerca"; // Prepara la query SQL per cercare il cliente in base alla scelta dell'utente
              
                using (MySqlCommand command = new MySqlCommand(query, connection))  // Crea un nuovo comando MySQL con la query e la connessione al db (procedura standard)
                {
                    command.Parameters.AddWithValue("@parametroRicerca", parametroRicerca); // Imposta il valore del parametro nel comando

                    using (MySqlDataReader reader = command.ExecuteReader())  // Esegui la query e ottieni i risultati nell'oggetto MySqlDataReader 'reader'
                    {
                        while (reader.Read())  // Leggi i risultati riga per riga
                        {
                            Cliente cliente = new Cliente(  // Crea un nuovo oggetto Cliente dai dati letti
                                reader.GetString("ID"),
                                reader.GetString("Nome"),
                                reader.GetString("Cognome"),
                                reader.GetString("Citta"),
                                reader.GetString("Sesso"),
                                reader.GetDateTime("DataDiNascita"));
                          
                            clientiTrovati.Add(cliente);  // Aggiungi il cliente trovato alla lista dei clienti trovati
                        }
                    }
                }
            }
            return clientiTrovati;  // Restituisce la lista dei clienti trovati
        }

        // AGGIUNGI //
        //public bool AggiungiCliente(Cliente nuovoCliente)
        //{
        //    using (MySqlConnection connection = new MySqlConnection(_connectionDB))
        //    {
        //        connection.Open();

        //        // Query per recuperare tutti gli ID dei clienti presenti nel database
        //        string queryRecuperaID = "SELECT ID FROM clienti";

        //        // Crea un oggetto MySqlCommand per eseguire la query di recupero degli ID
        //        using (MySqlCommand commandRecuperaID = new MySqlCommand(queryRecuperaID, connection))
        //        {
        //            // Legge gli ID dei clienti presenti nel database e li inserisce in una lista
        //            List<string> listaID = new List<string>();
        //            using (MySqlDataReader reader = commandRecuperaID.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    listaID.Add(reader.GetString(0));
        //                }
        //            }

        //            // Controlla se l'ID del nuovo cliente è già presente nella lista degli ID presenti nel database
        //            if (listaID.Contains(nuovoCliente.ID))
        //            {
        //                return false; // ID già presente nel database
        //            }
        //        }

        //        // Query per inserire il nuovo cliente nel database
        //        string queryInserimento = "INSERT INTO clienti (ID, Nome, Cognome, Citta, Sesso, DataDiNascita) VALUES (@ID, @Nome, @Cognome, @Citta, @Sesso, @DataDiNascita)";

        //        // Crea un oggetto MySqlCommand per eseguire la query di inserimento del nuovo cliente
        //        using (MySqlCommand commandInserimento = new MySqlCommand(queryInserimento, connection))
        //        {
        //            // Aggiungi i valori dei parametri(@) al command(MySqlCommand) tramite AddWithValue
        //            // Ogni parametro viene impostato con il corrispondente valore dell'oggetto nuovoCliente
        //            commandInserimento.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = nuovoCliente.ID;
        //            commandInserimento.Parameters.Add("@Nome", MySqlDbType.VarChar, 50).Value = nuovoCliente.Nome;
        //            commandInserimento.Parameters.Add("@Cognome", MySqlDbType.VarChar, 50).Value = nuovoCliente.Cognome;
        //            commandInserimento.Parameters.Add("@Citta", MySqlDbType.VarChar, 50).Value = nuovoCliente.Citta;
        //            commandInserimento.Parameters.Add("@Sesso", MySqlDbType.VarChar, 1).Value = nuovoCliente.Sesso;
        //            // MySqlDbType.Date trasferisce la data al database nel formato predefinito MySql yyyy/mm/gg altrimenti avrei dovuto utilizzare il DateTime di .NET per la conversione
        //            commandInserimento.Parameters.Add("@DataDiNascita", MySqlDbType.Date).Value = nuovoCliente.DataDiNascita;

        //            // Esegue la query di inserimento del nuovo cliente
        //            commandInserimento.ExecuteNonQuery();
        //        }

        //        return true; // Inserimento del cliente avvenuto con successo
        //    } //la connessione al database viene chiusa automaticamente quando si esce dal blocco using
        //}


        public void AggiungiCliente(Cliente nuovoCliente)
        {
            // Dichiarazione della connessione al database all'interno di un blocco using,
            // che garantisce che la connessione venga chiusa correttamente alla fine del blocco
            using (MySqlConnection connection = new MySqlConnection(_connectionDB))
            {
                // Apertura della connessione
                connection.Open();

                // Dichiarazione della query per cercare un cliente con lo stesso ID
                string query = "SELECT COUNT(*) FROM Clienti WHERE ID = @ID";

                // Dichiarazione di un comando con la query e la connessione al database
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Impostazione del valore del parametro "@ID" nel comando
                    command.Parameters.AddWithValue("@ID", nuovoCliente.ID);

                    // Esecuzione della query e ottenimento del risultato
                    int count = Convert.ToInt32(command.ExecuteScalar());

                    // Controllo se esiste già un cliente con lo stesso ID nel database
                    if (count > 0)
                    {
                        // Se esiste, lancio un'eccezione con un messaggio di errore
                        throw new InvalidOperationException("L'ID del cliente esiste già nel database.");
                    }
                }

                // Dichiarazione della query per inserire il nuovo cliente
                string insertQuery = "INSERT INTO Clienti (ID, Nome, Cognome, Citta, Sesso, DataDiNascita) " +
                    "VALUES (@ID, @Nome, @Cognome, @Citta, @Sesso, @DataDiNascita)";

                // Dichiarazione di un comando con la query e la connessione al database
                using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                {
                    // Impostazione dei valori dei parametri nel comando
                    insertCommand.Parameters.AddWithValue("@ID", nuovoCliente.ID);
                    insertCommand.Parameters.AddWithValue("@Nome", nuovoCliente.Nome);
                    insertCommand.Parameters.AddWithValue("@Cognome", nuovoCliente.Cognome);
                    insertCommand.Parameters.AddWithValue("@Citta", nuovoCliente.Citta);
                    insertCommand.Parameters.AddWithValue("@Sesso", nuovoCliente.Sesso);
                    insertCommand.Parameters.AddWithValue("@DataDiNascita", nuovoCliente.DataDiNascita);

                    // Esecuzione della query di inserimento
                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    // Controllo se la query ha inserito correttamente il nuovo cliente
                    if (rowsAffected != 1)
                    {
                        // Se non è stata inserita nessuna riga (rowsAffected = 0) o più di una riga,
                        // lancio un'eccezione con un messaggio di errore
                        throw new InvalidOperationException("Errore durante l'inserimento del nuovo cliente.");
                    }
                }
            }
        }







        // MODIFICA //
        public void ModificaCliente(string id, Cliente clienteModificato) //in input i dati da modificare (clienteModificato)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionDB))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE Clienti SET Nome = @Nome, Cognome = @Cognome, Citta = @Citta, Sesso = @Sesso, DataDiNascita = @DataDiNascita WHERE ID = @ID", conn);

                cmd.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = id; //l'id non viene modificato passo semplicemente il valore in input per trovare il cliente
                cmd.Parameters.Add("@Nome", MySqlDbType.VarChar, 50).Value = clienteModificato.Nome;
                cmd.Parameters.Add("@Cognome", MySqlDbType.VarChar, 50).Value = clienteModificato.Cognome;
                cmd.Parameters.Add("@Citta", MySqlDbType.VarChar, 50).Value = clienteModificato.Citta;
                cmd.Parameters.Add("@Sesso", MySqlDbType.VarChar, 1).Value = clienteModificato.Sesso;
                cmd.Parameters.Add("@DataDiNascita", MySqlDbType.Date).Value = clienteModificato.DataDiNascita;

                cmd.ExecuteNonQuery();
            }
        }

        // ELIMINA //
        public bool EliminaCliente(string id)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionDB))
            {
                conn.Open();
                //conn serve a collegare il comando SQL che stai creando con il database (obbligatorio in MySqlCommand)
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Clienti WHERE ID = @ID", conn);
                cmd.Parameters.Add("@ID", MySqlDbType.VarChar, 5).Value = id; // aggiunge il parametro @ID alla query SQL e imposta il suo valore uguale a quello dell'input id

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        public bool VerificaIDUnivoco(string id)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionDB))
            {
                connection.Open();

                // Selezionara tutti gli ID dal database
                string query = "SELECT ID FROM clienti";

                // Crea un oggetto MySqlCommand, passando la query e la connessione al db (procedura standard)
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Legge gli ID dal database utilizzando il metodo ExecuteReader del command (MySqlCommand)
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // Controlla se l'ID cercato esiste già nella lista degli ID letti dal database
                        //Il metodo Read() sposta il cursore del lettore sulla riga successiva del risultato, ritornando true se ci sono altre righe disponibili.
                        while (reader.Read())
                        {
                            // GetString mi serve per leggere il valore della riga che cambierà di continuo grazie al while
                            if (id == reader.GetString(0)) // Lo 0 serve per indicare che deve leggere le rihe della colonna 0
                            {
                                return false; // L'ID non è univoco
                            }
                        }
                    }
                }
            }
            return true; // L'ID è univoco
        }
    }
}