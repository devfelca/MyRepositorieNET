using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SistemaCompras
{
    // Enum de prioridade para facilitar ordenação e exibição
    enum Prioridade
    {
        Alta,
        Media,
        Baixa
    }

    // Classe que representa uma Solicitação de Compra

    class SolicitacaoCompra
    {
        public string Numero { get; set; }       
        public string NomeSolicitante { get; set; }
        public string Item { get; set; }
        public int Quantidade { get; set; }
        public Prioridade Prioridade { get; set; }
        public DateTime DataSolicitacao { get; set; }

        public SolicitacaoCompra(string numero, string nomeSolicitante, string item,
                                  int quantidade, Prioridade prioridade)
        {
            Numero = numero;
            NomeSolicitante = nomeSolicitante;
            Item = item;
            Quantidade = quantidade;
            Prioridade = prioridade;
            DataSolicitacao = DateTime.Now;
        }

        // Exibe a solicitação formatada no console
        public void Exibir()
        {
            Console.WriteLine($"  Nº Solicitação : {Numero}");
            Console.WriteLine($"  Solicitante    : {NomeSolicitante}");
            Console.WriteLine($"  Item           : {Item}");
            Console.WriteLine($"  Quantidade     : {Quantidade}");
            Console.WriteLine($"  Prioridade     : {Prioridade}");
            Console.WriteLine($"  Data           : {DataSolicitacao:dd/MM/yyyy HH:mm}");
        }
    }

    // Classe responsável por ler o arquivo e gerar protocolos
 
    class GerenciadorSolicitacoes
    {
        private List<SolicitacaoCompra> _solicitacoes = new List<SolicitacaoCompra>();
        private int _contadorProtocolo = 0;


        // Gera um número de solicitação sequencial no formato COMP-XXXX/AAAA >>> Poderia ser melhorado para incluir mais informações, como o mês ou um código do departamento, mas para simplicidade, mantive apenas o ano e um contador sequencial.

        private string GerarNumero()
        {
            _contadorProtocolo++;
            int ano = DateTime.Now.Year;
            return $"COMP-{_contadorProtocolo:D4}/{ano}";
        }


        // Converte a string de prioridade para o enum, com tolerância a acentos >>> Para facilitar o código > Podemos melhorar o tratamento

        private Prioridade ParsePrioridade(string texto)
        {
            return texto.Trim().ToLower() switch
            {
                "alta"  => Prioridade.Alta,
                "media" => Prioridade.Media,
                "média" => Prioridade.Media,
                "baixa" => Prioridade.Baixa,
                _       => Prioridade.Baixa   // valor padrão para texto não reconhecido
            };
        }


        // Lê o arquivo .txt e carrega as solicitações na lista interna.
        // Cada linha deve ter o formato: NomeSolicitante;Item;Quantidade;Prioridade

        public void CarregarArquivo(string caminho)
        {
            if (!File.Exists(caminho))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERRO] Arquivo não encontrado: {caminho}");
                Console.ResetColor();
                return;
            }

            string[] linhas = File.ReadAllLines(caminho);
            int linhaAtual = 0;

            foreach (string linha in linhas)
            {
                linhaAtual++;

                // Ignora linhas vazias ou comentários
                if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith("#"))
                    continue;

                string[] partes = linha.Split(';');

                if (partes.Length < 4)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[AVISO] Linha {linhaAtual} ignorada (formato inválido): {linha}");
                    Console.ResetColor();
                    continue;
                }

                string nomeSolicitante = partes[0].Trim();
                string item           = partes[1].Trim();
                Prioridade prioridade = ParsePrioridade(partes[3]);

                // Valida e converte a quantidade
                if (!int.TryParse(partes[2].Trim(), out int quantidade) || quantidade <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[AVISO] Linha {linhaAtual} com quantidade inválida. Usando 1.");
                    Console.ResetColor();
                    quantidade = 1;
                }

                string numero = GerarNumero();
                var solicitacao = new SolicitacaoCompra(numero, nomeSolicitante, item, quantidade, prioridade);
                _solicitacoes.Add(solicitacao);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] {_solicitacoes.Count} solicitação(ões) carregada(s) do arquivo.\n");
            Console.ResetColor();
        }


        /// Exibe todas as solicitações ordenadas conforme as opcoes disponiveis

        public void ExibirSolicitacoes(string criterioOrdenacao)
        {
            if (_solicitacoes.Count == 0)
            {
                Console.WriteLine("Nenhuma solicitação para exibir.");
                return;
            }

            IEnumerable<SolicitacaoCompra> ordenadas;

            if (criterioOrdenacao == "item")
                ordenadas = _solicitacoes.OrderBy(s => s.Item);
            else
                ordenadas = _solicitacoes.OrderBy(s => s.NomeSolicitante);

            string titulo = criterioOrdenacao == "item"
                ? "ORDENADAS POR ITEM"
                : "ORDENADAS POR SOLICITANTE";

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  SOLICITAÇÕES DE COMPRA — {titulo}");
            Console.ResetColor();

            int contador = 0;
            foreach (var s in ordenadas)
            {
                contador++;
                Console.WriteLine($"\n  [{contador}]");
                s.Exibir();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Total: {contador} solicitação(ões)");
            Console.ResetColor();
        }
    }

    // Ponto de entrada

    class Program
    {
        static void Main(string[] args)
        {
            // Cabeçalho
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("SISTEMA DE SOLICITAÇÃO DE COMPRAS GPQ — 2026");
            Console.ResetColor();

            // Define o caminho do arquivo (diretório do projeto >>> Tive problema com o path locale geral que identifica, erro no VB provalvelmente e não lembrei a sintexe correta para o path do projeto, então deixei o caminho do diretório atual, onde o .exe é gerado, e coloquei o .txt lá para facilitar)
            string caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "solicitacoes.txt");

            // Cria o gerenciador e carrega os dados 
            var gerenciador = new GerenciadorSolicitacoes();
            gerenciador.CarregarArquivo(caminhoArquivo);

            // Menu de ordenação >>>> Creio que aqui poderia ser um tratamento melhor, mas para fins de simplicidade e clareza, deixei um menu básico.
            bool continuar = true;
            while (continuar)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Ordenar solicitações por:");
                Console.WriteLine("  [1] Item");
                Console.WriteLine("  [2] Solicitante");
                Console.WriteLine("  [0] Sair");
                Console.Write("\nEscolha uma opção: ");
                Console.ResetColor();

                string? opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        gerenciador.ExibirSolicitacoes("item");
                        break;
                    case "2":
                        gerenciador.ExibirSolicitacoes("solicitante");
                        break;
                    case "0":
                        continuar = false;
                        Console.WriteLine("\nEncerrando o sistema. Até logo!");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[AVISO] Opção inválida. Tente novamente.\n");
                        Console.ResetColor();
                        break;
                }
            }
        }
    }
}
