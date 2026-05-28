import os

# ---------------------------------------------------------
# Configurações do Exportador
# ---------------------------------------------------------
PROJECT_ROOT = '.'  # Diretório raiz a analisar
OUTPUT_FILE = 'project_context.md'

# Diretórios que não nos interessam para a lógica de negócio
IGNORE_DIRS = {
    '.git', '.vs', 'bin', 'obj', 'node_modules', 
    'Migrations', 'wwwroot', 'Properties', '.idea'
}

# Ficheiros específicos a ignorar (ruído)
IGNORE_FILES = {
    'package-lock.json', 'launchSettings.json', 'appsettings.Development.json'
}

# Extensões que contêm a lógica e arquitetura que precisamos de discutir
RELEVANT_EXTENSIONS = {'.cs', '.cshtml', '.json'}

# Limite de tamanho por ficheiro (em bytes) - Ex: 50KB
# Evita exportar ficheiros gerados massivos que quebrem a nossa comunicação
MAX_FILE_SIZE = 50 * 1024 

def generate_markdown():
    print("A iniciar a recolha de contexto do projeto...")
    
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as md_file:
        md_file.write("# Contexto do Projeto E-Sports (ASP.NET Core)\n\n")
        md_file.write("> Ficheiro auto-gerado para partilha de contexto de código.\n\n")

        processed_files = 0

        for root, dirs, files in os.walk(PROJECT_ROOT):
            # Modifica a lista dirs in-place para o os.walk ignorar pastas desnecessárias
            dirs[:] = [d for d in dirs if d not in IGNORE_DIRS]

            for file in files:
                if file in IGNORE_FILES:
                    continue

                _, ext = os.path.splitext(file)
                if ext in RELEVANT_EXTENSIONS:
                    file_path = os.path.join(root, file)

                    # Ignorar ficheiros demasiado grandes
                    if os.path.getsize(file_path) > MAX_FILE_SIZE:
                        print(f"Ignorado (Muito grande): {file_path}")
                        continue

                    # Escreve o cabeçalho do ficheiro no Markdown
                    md_file.write(f"## Ficheiro: `{os.path.relpath(file_path, PROJECT_ROOT)}`\n\n")

                    # Mapeamento simples para syntax highlighting adequado
                    if ext == ".cs":
                        lang = "csharp"
                    elif ext == ".cshtml":
                        lang = "html" # Razor / HTML
                    elif ext == ".json":
                        lang = "json"
                    else:
                        lang = ""

                    md_file.write(f"```{lang}\n")
                    try:
                        with open(file_path, 'r', encoding='utf-8') as f:
                            md_file.write(f.read())
                        processed_files += 1
                    except Exception as e:
                        md_file.write(f"// Erro ao ler ficheiro: {e}")
                    
                    md_file.write("\n```\n\n")

    print(f"Sucesso! {processed_files} ficheiros exportados para '{OUTPUT_FILE}'.")

if __name__ == "__main__":
    generate_markdown()