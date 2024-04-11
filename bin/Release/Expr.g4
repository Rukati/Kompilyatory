grammar Expr;

prog: function* 'def Main' START_BODY stat* END_BODY'.' ;

stat: initialization END 
    | print
    | if
    | while END
    | doWhile END
    | changeValue END
    | for END
    | callFunc END
    ;
      
callFunc: funcID '(' (argc)*')';
funcID: ID;
argc: arguments (',' argc*)? ;
arguments: ('$'ID | NUMBER | expr);

initialization: TYPE ID (ASSIGNMENT (expr | callFunc))? ;

function: 'def' ID LPAREN parameter RPAREN '->' funType START_BODY stat* ('return' return END)? END_BODY'.';
parameter: (TYPE ID (',' parameter)*)?;
funType: TYPE | 'void';
return: '$'ID | NUMBER | expr ;

for : 'for' LPAREN for_init? ';' equation ';' changeValue? RPAREN START_BODY for_body END_BODY;
for_init: TYPE ID ASSIGNMENT expr;
for_body: (stat)* ('return' return END)?;

if : 'if' LPAREN equation RPAREN 'then' START_BODY ifBody END_BODY ('else' START_BODY elseBody END_BODY)? END;
ifBody: (stat)* ('return' return END)?;
elseBody: (stat)* ('return' return END)?;

doWhile : 'do' START_BODY whileBody END_BODY 'while' LPAREN equation RPAREN;

while : 'while' LPAREN equation RPAREN 'do' START_BODY whileBody END_BODY;
whileBody: (stat)* ('return' return END)?;

changeValue : '$' ID ASSIGNMENT expr;

START_BODY:'begin';
END_BODY: 'end';

equation: expr op=RELOP expr;
   
RELOP
   : EQ_EQ
   | GT
   | LT
   | NOT_EQ
   | LE
   | GE
   ;

print: PRINT LPAREN print_arguments RPAREN END;

PRINT: 'writeln' | 'write'; 
expr: LPAREN (expr) RPAREN
    | expr (MUL | SEP) expr
    | expr (ADD | SUB) expr 
    | '$'ID
    | NUMBER
    ;
print_arguments: ((LINE | '$'ID | NUMBER | (('('TYPE')')? expr)) (',' print_arguments)*)?;


TYPE: INTEGER | FLOAT ;

LINE: '"' (LETTER | PUNCTUATION | RELOP | EMPTY | DIGIT)+ '"';

ID: LETTER (LETTER | DIGIT)*;

NUMBER: ('-')? DIGIT+ ('.' DIGIT+)?;

fragment DIGIT : [0-9];
fragment LETTER : [a-zA-Z];
fragment PUNCTUATION : [!.,;:];

LPAREN: '(';
RPAREN: ')';

INTEGER: 'int';
FLOAT: 'float';

ADD: '+';
SUB: '-';
SEP: '/';
MUL: '*';


EQ_EQ: '==';
NOT_EQ: '!=';
GT: '>';
LT: '<';
LE : '<=';
GE: '>=';
ASSIGNMENT : '=';

END : ';';

EMPTY: [ \n\t\r] -> skip;