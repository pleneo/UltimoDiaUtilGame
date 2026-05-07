Template de Game Design Document (GDD)




Título do Jogo: Últimos Dias Úteis
Integrantes:

Jean de Souza Morais,
Rayla Almeida Freitas,

João Pedro Sidou Aragão,
Plínio Gomes de Oliveira


1. Visão Geral
   Gênero: Puzzle, Simulador Burocrático
   Plataforma(s):  Windows
   Engine: Unity
   Público-alvo: Adolescentes e Adultos
   Resumo do Conceito: Você é um aluno a um semestre de se formar na Universidade de Fortaleza, e por ser desorganizado com suas finanças ficou devendo muitas mensalidades e agora estará impedido de se formar se não conseguir quitar a dívida. Para isso, conseguiu um emprego na Reitoria da própria universidade, tendo que diariamente lidar com burocracias de alunos novos e veteranos. Você será encarregado de validar papéis de matrícula, trancamento de cadeiras, pagamento de mensalidades, troca de curso e emissão de certificado de conclusão, onde cada uma dessas ações terá documentos e informações cruciais para serem validados. Defira e indefira documentos, mas tenha cuidado: espertinhos tentarão falsificar documentos para serem aprovados, se você deixar passar será severamente penalizado.
   O jogo terá como característica ser 2D (em perspectiva ortográfica), o foco principal será a cabine de atendimento onde o player irá passar ciclos de dias lidando com novos tipos de atividades a serem feitas e incrementação de dificuldade para as que já existem.
   Visão Artística: O design visual será pixelizado, com baixa paleta de cores vivas para contrastar com o ambiente sombrio da universidade em crise. O jogo terá como tom narrativo falas que reverberam rigidez existente em um ambiente corporativo ao mesmo tempo que apresenta o humor satírico dado o momento conturbado da corporação (funcionários destreinados, clientes abusados, etc).






2. Mecânicas de Jogo
   Jogabilidade Central:
   A característica central é a de deferimento de documentos trazidos até você, essa decisão será tomada a partir da problemática dada pelo aluno através de balões de fala, reproduzindo sons de gibberish, englobando os seguintes elementos:
   O player terá um livro de regras onde será dito suas atribuições e como realizar as ações
   O livro de regras diariamente será atualizado, trazendo novos elementos e evoluindo a dificuldade do jogo
   O player deverá, dentro das burocracias resolvidas pela reitoria listadas no livro de regras, verificar a legitimidade dos documentos entregues, verificar se há documentos faltantes e caso tenha contato direto com dinheiro, verificar se é falso
   Personagens específicos trarão dilemas morais à história, onde mesmo que tenham questões que possam reprovar os documentos tragos dentro dos limites burocráticos, podem tocar o coração do player e terem sua documentação aprovada, trazendo ou não recompensas ou advertências ao jogador por essas ações

Objetivos e Progressão:

	O jogo é estruturado em ciclos diários de trabalho, a progressão é incremental e gradativamente feita através de adições de trabalhos que elevam a carga cognitiva, trazendo também maior complexidade visual e sistemática. 
Sendo o objetivo:

O jogador deve validar uma fila de alunos dentro de um limite de tempo pré determinado
Ao final de cada dia, o jogo calcula o saldo monetário do jogador
A condição de vitória é chegar ao último dia conseguindo quitar toda sua dívida com a universidade

Há também um escalonamento diário de dificuldade, trazendo complexidade através da quantidade de informações que o player será exposto


Sistema de Recompensas:

	Há fatores cruciais que regem o sistema de recompensas no game
Julgamento correto dos casos: cada julgamento correto possui uma recompensa monetária que será somada ao fim do dia. Quanto mais julgamentos forem corretamente feitos em um dia, mais o jogador irá ganhar dinheiro
Julgamento incorreto dos casos: julgamentos incorretos aumentam o multiplicador de multa do jogador para aquele dia, penalizando monetariamente o jogador e o obrigando a economizar em questões pessoais muitas vezes cruciais (comida, higiene, etc)
Game Over: receber excessivas advertências ou ficar negativo em dinheiro acarretará em game over

Recursos Matemáticos:
Translação: movimentação de objetos no eixo x e y na bancada de trabalho
Z-Order: gerenciamento de camadas de objetos, deixando o último documento selecionado no topo de uma pilha de documentos
Point-in-Poygon: teste de verificação se as coordenadas do mouse (x,y) pertencem à área delimitada de um documento, trazendo feedback visual sobre essa ação e permitindo que o clique selecione o documento em questão
Fila de alunos: fila para gerenciar a entrada de cada NPC;

3. Mundo e Narrativa
   Cenário: O jogo se passa na reitoria da Unifor, um ambiente de luz baixa e burocrático, em um período pré-pandêmico
   História e Personagens:
   O principal enredo é a história do atendente da reitoria, novo secretário, em que ele deve juntar dinheiro e fazer suas economias a fim de quitar sua dívida com a faculdade,
   Menino Loiro: Quer inicialmente fazer sua matrícula na faculdade mas está enrolado com os documentos, acabou de sair do ensino médio; Repetente da Cadeira de Cálculo: deseja fazer o trancamento da disciplina, mas cria uma discussão pois não quer pagar a taxa de trancamento; Filha e Pai: querem fazer o pagamento da mensalidade, mas parecem muito suspeitos e ansiosos; Mulher Ruiva: Deseja trocar o seu curso e aluga o ouvido do personagem principal sobre suas questões emocionais; Aluno Formado: Foi buscar seu certificado e fica nostalgico sobre seu tempo na universidade.
   Elementos de Lore: Estamos num mundo pré-pandêmico onde todos estão com medo do novo vírus chegar à universidade.
4. Níveis e Ambientes
   Estrutura dos Níveis: Os níveis são organizados pelos dias da semana, a cada dia concluído a dificuldade fica mais difícil. A conclusão dos níveis desbloqueia novas habilidades e novos comandos no livro de regras.
   Design dos Ambientes: A Reitoria da Unifor com uma luz baixa, cadeiras pelo cômodo, um balcão à frente da visão do jogador. No balcão estão contidos livro de regras, carimbos, canetas e calendário. Ao olhar para o lado há ainda um quadro com avisos e notas de pessoas com casos especiais que devem ser mandadas diretamente para o superior do jogador.
   Desafios e Obstáculos: O jogador terá que verificar a data de validade dos documentos entregues e se estes documentos estão certos, fazer a validação de integridade física e visual de documentos onde se verificam os carimbos necessários, observar se a informação em um documento A está apresentada da mesma maneira no documento B, se o dinheiro é falso, e se aquela pessoa está no quadro de avisos onde casos especiais devem ser mandados ao superior.
5. Arte e Áudio
   Estilo Visual: Perspectiva: 2D ortográfica (top-down fixo na bancada) com Pixel Art estilizada (não hiper detalhada, foco em legibilidade)
   Paleta de cores:
   Base: tons frios e dessaturados (cinza, azul escuro, verde institucional)
   Destaques: cores vivas para elementos interativos (carimbos, erros, alertas), tendo feedback visual com verde = correto, vermelho = erro, amarelo = atenção
   Referência estética: semelhante a Papers, Please, mas com identidade universitária brasileira
   Design de Personagens: Pixel art simples com silhuetas bem definidas e características de exagero leve (estilo caricatura) para facilitar leitura rápida e expressões limitadas, mas marcantes (ansioso, bravo, triste)

Exemplos:
Alunos nervosos → animação de tremor
Funcionários → postura rígida
Importante tecnicamente também ter um sistema modular onde se tem a Cabeça +
corpo + acessórios separados → facilita variações sem refazer sprites

Trilha Sonora e Efeitos Sonoros: Lo-fi / ambiente corporativo repetitivo, que intensifica conforme o tempo do dia vai acabando, sendo importante também ter um feedback auditivo de reforço mecânico para o jogador saber se errou sem olhar. Efeitos sonoros essenciais:
Carimbo → som seco e satisfatório
Papel sendo movido
Clique de seleção
Som de erro (grave)
Som de acerto (leve e positivo)
Vozes “Gibberish” (tipo Animal Crossing)
6. Interface do Usuário (UI)
   Layout e Design: Organização fixa da tela:
   Centro: mesa de trabalho (documentos)
   Esquerda: fila de alunos
   Direita: livro de regras
   Topo: tempo do dia + dinheiro
   Lateral Direita: quadro de avisos
   Funcionalidade: Drag and Drop para documentos. Botões principais:
   Deferido
   Indeferido
   Encaminhar
   Hover mostra:
   zoom leve
   highlight (uso de Point-in-Polygon aqui)

Acessibilidade: Modo alto contraste, fonte legível (pixel mas clara), ícones + cores, ajuste de volume separado (música e efeitos).
7. Considerações Técnicas
   Motor de Jogo: Engine: Unity; Linguagem: C#; Render: 2D

Otimização: Uso de Object Pooling (para NPCs); Sprites em atlas (reduz draw calls); Evitar Update() pesado → usar eventos; Limitar física (quase não usar Rigidbody)
8. Cronograma e Orçamento
   Fases de Desenvolvimento: Metodologia Scrum:
   Fase 1 – Pré-produção (1–2 semanas)
   Finalizar GDD
   Protótipo básico (clicar + arrastar)
   Definir arte base
   Fase 2 – Protótipo Jogável (2–3 semanas)
   Sistema de documentos simples
   Aprovar/reprovar funcionando
   1 tipo de NPC
   Fase 3 – Core Gameplay (3–4 semanas)
   Livro de regras dinâmico
   Múltiplos tipos de validação
   Sistema de dinheiro
   Fase 4 – Conteúdo e Polimento (2–3 semanas)
   Personagens
   Dilemas morais
   UI final
   Sons
   Fase 5 – Testes e Ajustes (1–2 semanas)
   Balanceamento
   Correção de bugs
   Ajuste de dificuldade
   Equipe:
   João Pedro: Programação (core systems, lógica)
   Jean: Programação / UI
   Rayla: Arte / UI / Narrativa
   Plínio: Game Design  / balanceamento

